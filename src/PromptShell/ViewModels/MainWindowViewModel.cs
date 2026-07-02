using System;
using System.IO;
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
    private readonly IOutputInterpreterService _outputInterpreterService;
    
    [ObservableProperty]
    private string _inputCommand = string.Empty;
    
    [ObservableProperty]
    private string _terminalOutput = "PromptShell Ready... Ask me anything!";
    
    [ObservableProperty]
    private string _aiExplanation = string.Empty;

    [ObservableProperty] 
    private bool _isPendingApproval = false;
    
    [ObservableProperty]
    private string _pendingCommand = string.Empty;
    
    [ObservableProperty]
    private string _currentWorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    
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
        _outputInterpreterService = new OutputInterpreterService();
    }
    
    [RelayCommand]
    private async Task ExecuteTerminalCommandAsync()
    {
        if (string.IsNullOrWhiteSpace(InputCommand))
            return;
        
        string userRequest = InputCommand;
        InputCommand = string.Empty;
        AiExplanation = string.Empty;
        
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
                await RunAndAnalyzeCommandAsync(generatedCommand);
            }
        }
        catch (Exception ex)
        {
            TerminalOutput = $"[Fatal Error] An unexpected error occurred: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task ApproveCommandAsync()
    {
        if (string.IsNullOrWhiteSpace(PendingCommand)) 
            return;

        string cmdToRun = PendingCommand;
        ResetApprovalState();
        
        await RunAndAnalyzeCommandAsync(cmdToRun);
    }
    
    [RelayCommand]
    private void RejectCommand()
    {
        ResetApprovalState();
        TerminalOutput = "Command execution cancelled by user.";
    }
    
    private bool RequiresApproval(string command)
    {
        var tokens = command.Split(new[] { ' ', '"', '\'' }, StringSplitOptions.RemoveEmptyEntries);
        return tokens.Any(token => DestructiveKeywords.Contains(token)) || command.Contains(">") || command.Contains(">>");
    }
    
    private async Task RunAndAnalyzeCommandAsync(string command)
    {
        // Átadjuk a kiválasztott CurrentWorkingDirectory-t a szerviznek
        TerminalOutput = $"[System] Running command in zsh ({Path.GetFileName(CurrentWorkingDirectory)})...";
        var result = await _terminalService.ExecuteCommandAsync(command, CurrentWorkingDirectory);

        if (result.IsSuccessful)
        {
            TerminalOutput = $"[Raw Output]:\n{result.Output}";
        }
        else
        {
            TerminalOutput = $"[Raw Error Output - Exit Code: {result.ExitCode}]:\n{result.Error}";
        }

        TerminalOutput += "\n\n[AI] Analyzing result status...";
        string explanation = await _outputInterpreterService.InterpretResultAsync(command, result);
        
        AiExplanation = explanation;
        
        if (result.IsSuccessful)
        {
            TerminalOutput = $"[Success]: {command}\n\n{result.Output}";
        }
        else
        {
            TerminalOutput = $"[Failure]: {command}\n\n{result.Error}";
        }
    }
    
    private void ResetApprovalState()
    {
        PendingCommand = string.Empty;
        IsPendingApproval = false;
    }
}