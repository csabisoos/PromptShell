using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using PromptShell.Models;

namespace PromptShell.Services;

public class OutputInterpreterService : IOutputInterpreterService
{
    private readonly HttpClient _httpClient;
    private const string ModelName = "llama3";
    private const string BaseUrl = "http://localhost:11434/api/generate";
    
    public OutputInterpreterService()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }
    
    public async Task<string> InterpretResultAsync(string command, TerminalResult result, CancellationToken cancellationToken = default)
    {
        if (result == null) return string.Empty;
        
        string systemPrompt = "You are an elite, ultra-concise CLI output analyzer. " +
                              "Your job is to read the terminal command and its output, then summarize it in a professional, telegraphic style. " +
                              "CRITICAL: Be extremely brief. Maximum 1-2 short sentences. " +
                              "Do NOT use conversational filler like 'Sure, here is...', 'Based on the output...', or 'This command shows...'. " +
                              "If the output is just a single data point (e.g., folder size, IP address, user name), output ONLY that factual data in a clean format. " +
                              "Example for folder size: 'Size of [folder]: 4.2 GB'. " +
                              "If the command failed, output exactly what went wrong and give the exact command to fix it on a new line.";
        
        string rawPrompt = $"Command executed: {command}\n" +
                           $"Exit Code: {result.ExitCode}\n" +
                           $"Standard Output: {result.Output}\n" +
                           $"Standard Error: {result.Error}\n";

        var requestBody = new OllamaRequest(ModelName, rawPrompt, systemPrompt);

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(BaseUrl, requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);
            return ollamaResponse?.Response?.Trim() ?? "Failed to generate an analysis.";
        }
        catch (Exception ex)
        {
            return $"[AI Analysis Error]: Failed to connect to Ollama for the analysis: {ex.Message}";
        }
    }
}