using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PromptShell.Services;

namespace PromptShell.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ITerminalService _terminalService;
    private readonly IOllamaService _ollamaService;
    
    [ObservableProperty]
    private string _inputCommand = string.Empty;
    
    [ObservableProperty]
    private string _terminalOutput = "PromptShell Ready... Ask me anything!";

    [ObservableProperty] 
    private bool _isPendingApproval = false;
    
    [ObservableProperty]
    private string _pendingCommand = string.Empty;
    
    private static readonly string[] DestructiveKeywords = 
    { 
        "rm", "mkdir", "touch", "mv", "cp", "chmod", "chown", 
        "git commit", "git push", "git rm", "git merge", "git rebase", 
        "dd", "sudo", ">>", ">", "sed", "awk", "tee" 
    };
    
    public MainWindowViewModel()
    {
        _terminalService = new TerminalService();
        _ollamaService = new OllamaService();
    }

    /// <summary>
    /// Processes the current terminal input command asynchronously.
    /// If the input is empty or contains only whitespace, the method exits without execution.
    /// Otherwise, the input is sent for translation into a terminal command, which is either marked for approval or executed directly.
    /// In the event of errors during translation or execution, an appropriate error message is displayed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [RelayCommand]
    private async Task ExecuteTerminalCommandAsync()
    {
        if (string.IsNullOrWhiteSpace(InputCommand))
            return;
        
        string userRequest = InputCommand;
        InputCommand = string.Empty;
        
        try
        {
            TerminalOutput = $"[AI] Interpreting request: \"{userRequest}\"...";
            string generatedCommand = await _ollamaService.GenerateCommandAsync(userRequest);

            if (string.IsNullOrWhiteSpace(generatedCommand) || generatedCommand.StartsWith("# Error"))
            {
                TerminalOutput = generatedCommand.StartsWith("# Error") 
                    ? generatedCommand 
                    : "[AI Error] Could not translate this request into a valid terminal command.";
                return;
            }
            
            if (RequiresApproval(generatedCommand))
            {
                PendingCommand = generatedCommand;
                IsPendingApproval = true;
                TerminalOutput = $"[AI] Generated command: {generatedCommand}\n\n⚠️ WARNING: This command can modify your system or files. Approval required!";
            }
            else
            {
                await RunCommandDirectlyAsync(generatedCommand);
            }
        }
        catch (Exception ex)
        {
            TerminalOutput = $"[Fatal Error] An unexpected error occurred: {ex.Message}";
        }
    }

    /// <summary>
    /// Approves and executes a pending terminal command asynchronously.
    /// If the command is empty or only contains whitespace, the method exits without execution.
    /// Otherwise, the pending command is processed, approval state reset, and the command executed directly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [RelayCommand]
    private async Task ApproveCommandAsync()
    {
        if (string.IsNullOrWhiteSpace(PendingCommand))
            return;

        string cmdToRun = PendingCommand;
        ResetApprovalState();
        
        await RunCommandDirectlyAsync(cmdToRun);
    }

    /// <summary>
    /// Cancels the execution of a pending terminal command by resetting the approval state
    /// and updating the terminal output to inform the user of the cancellation.
    /// </summary>
    [RelayCommand]
    private void RejectCommand()
    {
        ResetApprovalState();
        TerminalOutput = "Command execution cancelled by user.";
    }

    /// <summary>
    /// Determines whether the given command requires user approval before execution.
    /// A command requires approval if it contains destructive keywords or redirection operators
    /// that can potentially modify the system or files.
    /// </summary>
    /// <param name="command">The terminal command to evaluate.</param>
    /// <returns>Returns true if the command requires approval; otherwise, false.</returns>
    private bool RequiresApproval(string command)
    {
        var tokens = command.Split(new[] { ' ', '"', '\'' }, StringSplitOptions.RemoveEmptyEntries);
        return tokens.Any(token => DestructiveKeywords.Contains(token)) || command.Contains(">") || command.Contains(">>");
    }

    /// <summary>
    /// Executes a terminal command directly without requiring prior user approval.
    /// The command is run in the zsh shell, and the terminal output or errors are captured and displayed.
    /// </summary>
    /// <param name="command">The terminal command to execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RunCommandDirectlyAsync(string command)
    {
        TerminalOutput = $"[System] Running command in zsh: {command}...";
        var result = await _terminalService.ExecuteCommandAsync(command);

        if (result.IsSuccessful)
        {
            TerminalOutput = $"[Executed]: {command}\n\n{result.Output}";
        }
        else
        {
            TerminalOutput = $"[Executed]: {command}\n\n[ERROR (Exit Code: {result.ExitCode})]\n{result.Error}";
        }
    }

    /// <summary>
    /// Resets the approval state for the pending command by clearing the pending command value
    /// and setting the approval requirement status to false.
    /// </summary>
    private void ResetApprovalState()
    {
        PendingCommand = string.Empty;
        IsPendingApproval = false;
    }
}