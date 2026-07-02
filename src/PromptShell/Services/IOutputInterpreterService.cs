using System.Threading;
using System.Threading.Tasks;
using PromptShell.Models;

namespace PromptShell.Services;

public interface IOutputInterpreterService
{
    Task<string> InterpretResultAsync(string command, TerminalResult result, CancellationToken cancellationToken = default);
}