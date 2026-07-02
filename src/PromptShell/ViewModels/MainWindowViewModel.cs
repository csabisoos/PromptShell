using System;
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
    
    public MainWindowViewModel()
    {
        _terminalService = new TerminalService();
        _ollamaService = new OllamaService();
    }

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

            TerminalOutput = $"[AI] Generated command: {generatedCommand}\n[System] Running command in zsh...";
            var result = await _terminalService.ExecuteCommandAsync(generatedCommand);

            if (result.IsSuccessful)
            {
                TerminalOutput = $"[Executed]: {generatedCommand}\n\n{result.Output}";
            }
            else
            {
                TerminalOutput = $"[Executed]: {generatedCommand}\n\n[ERROR (Exit Code: {result.ExitCode})]\n{result.Error}";
            }
        }
        catch (Exception ex)
        {
            TerminalOutput = $"[Fatal Error] An unexpected error occurred: {ex.Message}";
        }
    }
}