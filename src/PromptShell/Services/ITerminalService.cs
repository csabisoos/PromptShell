using System.Threading;
using System.Threading.Tasks;
using PromptShell.Models;

namespace PromptShell.Services;

public interface ITerminalService
{
    /// <summary>
    /// Executes a command in the terminal and returns the structured result.
    /// </summary>
    /// <param name="command">The native command to run.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A TerminalResult object containing the output and status of the command.</returns>
    Task<TerminalResult> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);
}