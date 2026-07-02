using System.Threading;
using System.Threading.Tasks;

namespace PromptShell.Services;

public interface IOllamaService
{
    Task<string> GenerateCommandAsync(string userPrompt, string? directoryContext, CancellationToken cancellationToken = default);
}