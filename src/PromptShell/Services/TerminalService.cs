using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PromptShell.Models;

namespace PromptShell.Services;

public class TerminalService : ITerminalService
{
    public async Task<TerminalResult> ExecuteCommandAsync(string command, string? workingDirectory = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return new TerminalResult(string.Empty, "Command cannot be null or whitespace.", -1);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "/bin/zsh",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        
        if (!string.IsNullOrWhiteSpace(workingDirectory) && Directory.Exists(workingDirectory))
        {
            startInfo.WorkingDirectory = workingDirectory;
        }
        
        startInfo.ArgumentList.Add("-c");
        startInfo.ArgumentList.Add(command);
        
        using var process = new Process { StartInfo = startInfo };

        try
        {
            if (!process.Start())
            {
                return new TerminalResult(string.Empty, "Failed to start the process.", -1);
            }
            
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);
            
            string output = await outputTask;
            string error = await errorTask;
            
            return new TerminalResult(output.Trim(), error.Trim(), process.ExitCode);
        }
        catch (Exception ex)
        {
            return new TerminalResult(string.Empty, $"Internal process error: {ex.Message}", -1);
        }
        
    }
}