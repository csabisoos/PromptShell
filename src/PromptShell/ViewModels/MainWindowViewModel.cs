using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PromptShell.Services;

namespace PromptShell.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ITerminalService _terminalService;
    
    [ObservableProperty]
    private string _inputCommand = string.Empty;
    
    [ObservableProperty]
    private string _terminalOutput = "PromptShell Ready... Enter a command below.";
    
    public MainWindowViewModel()
    {
        _terminalService = new TerminalService();
    }

    [RelayCommand]
    private async Task ExecuteTerminalCommandAsync()
    {
        if (string.IsNullOrWhiteSpace(InputCommand))
            return;
        
        TerminalOutput = "Running command...";
        
        var result = await _terminalService.ExecuteCommandAsync(InputCommand);

        if (result.IsSuccessful)
        {
            TerminalOutput = result.Output;
        }
        else
        {
            TerminalOutput = $"[ERROR (Exit Code: {result.ExitCode})]\n{result.Error}";
        }
        
        InputCommand = string.Empty;
    }
}