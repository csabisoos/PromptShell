using System.Threading;
using System.Threading.Tasks;

namespace PromptShell.Services;

/// <summary>
/// Interface for interacting with the Ollama API to generate commands based on user prompts.
/// </summary>
public interface IOllamaService
{
    /// <summary>
    /// Generates a command based on the provided user prompt by interacting with the Ollama API.
    /// </summary>
    /// <param name="userPrompt"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> GenerateCommandAsync(string userPrompt, CancellationToken cancellationToken = default);
}