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
        
        string systemPrompt = "You are an expert IT system administrator and developer assistant. " +
                              "Your task is to analyze the provided macOS terminal command execution result and explain it to the user in a friendly, clear, human-readable way. " +
                              "If the command was successful, provide a brief summary of what happened. " +
                              "If the command failed (ExitCode != 0 or non-empty error), explain what went wrong and suggest a clear, exact fix. " +
                              "Keep your explanation concise and well-structured.";
        
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