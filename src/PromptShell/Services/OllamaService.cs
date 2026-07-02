using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using PromptShell.Models;

namespace PromptShell.Services;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private const string ModelName = "llama3";
    private const string BaseUrl = "http://localhost:11434/api/generate";

    public OllamaService()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    public async Task<string> GenerateCommandAsync(string userPrompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
            return string.Empty;
        
        string systemPrompt = "You are an expert macOS zsh shell assistant. " +
                              "Your task is to translate the user's natural language request into a single, valid, executable macOS zsh terminal command. " +
                              "CRITICAL: Respond ONLY with the raw command. " +
                              "Do NOT include any explanations, do NOT include markdown formatting, do NOT include backticks (```), and do NOT add introductory text. " +
                              "If the request cannot be turned into a command, return an empty string.";
        
        var requestBody = new OllamaRequest(ModelName, userPrompt, systemPrompt);

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(BaseUrl, requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);

            return ollamaResponse?.Response?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            return $"# Error contacting Ollama: {ex.Message}";
        }
    }
}