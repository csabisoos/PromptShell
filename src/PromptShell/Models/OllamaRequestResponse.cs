using System.Text.Json.Serialization;

namespace PromptShell.Models;

/// <summary>
/// Represents a request to the Ollama API, containing the model, prompt, system prompt, and streaming option.
/// </summary>
/// <param name="Model"></param>
/// <param name="Prompt"></param>
/// <param name="SystemPrompt"></param>
/// <param name="Stream"></param>
public record OllamaRequest (
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("system")] string SystemPrompt,
    [property: JsonPropertyName("stream")] bool Stream = false);
    
/// <summary>
/// Represents a response from the Ollama API, containing the response text and a flag indicating whether the response is complete.
/// </summary>
/// <param name="Response"></param>
/// <param name="Done"></param>
public record OllamaResponse (
    [property: JsonPropertyName("response")] string Response,
    [property: JsonPropertyName("done")] bool Done);