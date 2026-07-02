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

    public async Task<string> GenerateCommandAsync(string userPrompt, string? directoryContext = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
            return string.Empty;
        
        string systemPrompt = "You are a strict, cross-platform software development CLI assistant. " +
                              "Your ONLY job is to convert the user's request into a single executable shell command based on the provided directory context.\n\n" +
                              "CRITICAL RULES FOR CLARIFICATION:\n" +
                              "1. If the user asks a generic action (like 'build', 'run', 'test', or 'deploy') and the directory context contains MULTIPLE distinct project or solution files, you MUST NOT guess which one to use. You MUST ask a clarifying question.\n" +
                              "2. When asking a question, your response MUST strictly start with a '?' character, followed by your question. Example: '?I found multiple separate projects in this directory. Which one do you want to target?'\n" +
                              "3. If the request is clear and matches a single project in the context, respond ONLY with the raw command. No explanations, no markdown, no backticks (```).";
        
        string fullPrompt = string.Empty;
        if (!string.IsNullOrWhiteSpace(directoryContext))
        {
            fullPrompt += $"[DIRECTORY CONTEXT]:\nFiles present in current folder: {directoryContext}\n\n";
        }
        fullPrompt += $"[USER REQUEST]: {userPrompt}";
        
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