namespace PromptShell.Models;

/// <summary>
/// Represents the structured result of an executed command in the terminal.
/// </summary>
public record TerminalResult(string Output, string Error, int ExitCode)
{
    /// <summary>
    /// Indicates whether the command executed successfully (exit code 0).
    /// </summary>
    public bool IsSuccessful => ExitCode == 0;
}