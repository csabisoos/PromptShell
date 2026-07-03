using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PromptShell.Models;

namespace PromptShell.Services;

public class AiInferenceService : IAiInferenceService
{
    private readonly HttpClient _httpClient;
    private const string CentralProxyUrl = "https://your-shared-proxy.com/api/v1/chat/completions";

    public AiInferenceService()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
    }

    public async Task<string> GenerateCommandAsync(string userPrompt, string? directoryContext, AppSettings settings, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userPrompt)) return string.Empty;
        
        string manualContext = string.Empty;
        string firstWord = userPrompt.Trim().Split(' ')[0].ToLower();
        if (firstWord.Length > 1 && !firstWord.StartsWith("."))
        {
            manualContext = await GetTerminalManualAsync(firstWord);
        }

        string systemPrompt = "You are a strict, cross-platform software development CLI assistant. " +
                              "Your ONLY job is to convert the user's request into a single executable shell command based on the provided directory context.\n\n" +
                              "CRITICAL RULES:\n" +
                              "1. If the request is ambiguous or multiple project files exist, you MUST ask a clarifying question starting with '?'.\n" +
                              "2. Respond ONLY with the raw command. No explanations, no markdown, no backticks.";

        string fullPrompt = string.Empty;
        if (!string.IsNullOrWhiteSpace(directoryContext)) fullPrompt += $"[DIRECTORY CONTEXT]:\nFiles: {directoryContext}\n\n";
        if (!string.IsNullOrWhiteSpace(manualContext)) fullPrompt += $"[MANUAL FOR '{firstWord}']:\n{manualContext}\n\n";
        fullPrompt += $"[USER REQUEST]: {userPrompt}";

        return await ExecuteHttpInferenceAsync(systemPrompt, fullPrompt, settings, cancellationToken);
    }

    public async Task<string> InterpretResultAsync(string command, TerminalResult result, AppSettings settings, CancellationToken cancellationToken = default)
    {
        string systemPrompt = "You are an elite, ultra-concise CLI output analyzer. " +
                              "CRITICAL: Be extremely brief. Maximum 1-2 short sentences. No filler. " +
                              "If the output is a single data point, output ONLY that data.";

        string rawPrompt = $"Command: {command}\nExit Code: {result.ExitCode}\nSTDOUT: {result.Output}\nSTDERR: {result.Error}";

        return await ExecuteHttpInferenceAsync(systemPrompt, rawPrompt, settings, cancellationToken);
    }

    private async Task<string> ExecuteHttpInferenceAsync(string system, string user, AppSettings settings, CancellationToken cancellationToken)
    {
        string targetUrl;
        string modelName;
        var request = new HttpRequestMessage(HttpMethod.Post, "");

        if (settings.AiProvider == "Proxy")
        {
            targetUrl = CentralProxyUrl;
            modelName = "llama3-8b-8192";
        }
        else if (settings.AiProvider == "CustomApi")
        {
            targetUrl = settings.CustomApiUrl;
            modelName = settings.CustomModelName;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.CustomApiKey);
        }
        else
        {
            targetUrl = "http://localhost:11434/api/generate";
            modelName = "llama3";
        }

        request.RequestUri = new Uri(targetUrl);
        
        object requestBody;
        if (settings.AiProvider == "Ollama")
        {
            requestBody = new
            {
                model = modelName,
                prompt = user,
                system = system,
                stream = false 
            };
        }
        else
        {
            requestBody = new
            {
                model = modelName,
                messages = new[]
                {
                    new { role = "system", content = system },
                    new { role = "user", content = user }
                },
                temperature = 0.1
            };
        }

        request.Content = JsonContent.Create(requestBody);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            
            if (settings.AiProvider == "Ollama")
            {
                return jsonDoc.RootElement.GetProperty("response").GetString()?.Trim() ?? string.Empty;
            }
            else
            {
                return jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()?.Trim() ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            return $"# Error contacting AI provider ({settings.AiProvider}): {ex.Message}";
        }
    }

    private async Task<string> GetTerminalManualAsync(string commandName)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/man",
                Arguments = commandName,
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
            };
            using var process = new Process { StartInfo = startInfo };
            if (!process.Start()) return string.Empty;
            string rawManual = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            if (string.IsNullOrWhiteSpace(rawManual) || rawManual.Length < 20) return string.Empty;
            return rawManual.Substring(0, Math.Min(rawManual.Length, 1200)) + "... [Truncated]";
        }
        catch { return string.Empty; }
    }
}