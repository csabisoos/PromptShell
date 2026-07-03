using System.Threading;
using System.Threading.Tasks;
using PromptShell.Models;

namespace PromptShell.Services;

public interface IAiInferenceService
{
    /// <summary>
    /// Translates a natural language user request into a single executable shell command.
    /// </summary>
    Task<string> GenerateCommandAsync(string userPrompt, string? directoryContext, AppSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Interprets the command execution results into a brief, telegraphic summary.
    /// </summary>
    Task<string> InterpretResultAsync(string command, TerminalResult result, AppSettings settings, CancellationToken cancellationToken = default);
}