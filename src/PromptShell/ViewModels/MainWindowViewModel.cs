using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PromptShell.Models;
using PromptShell.Services;

namespace PromptShell.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ITerminalService _terminalService;
    private readonly IAiInferenceService _aiInferenceService;

    private readonly List<string> _history = new();
    private int _historyIndex = -1;
    private string _lastAiQuestion = string.Empty;

    private static readonly string CommandsLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history_commands.log");
    private static readonly string SessionLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "latest_session.log");
    
    public ObservableCollection<ChatMessage> ChatTimeline { get; } = new();

    [ObservableProperty] private AppSettings _settings = new();
    [ObservableProperty] private string _inputCommand = string.Empty;
    [ObservableProperty] private bool _isPendingApproval = false;
    [ObservableProperty] private string _pendingCommand = string.Empty;
    [ObservableProperty] private string _currentWorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    [ObservableProperty] private string _terminalOutput = string.Empty;
    [ObservableProperty] private string _aiExplanation = string.Empty;
    
    public Func<string, Task>? CopyToClipboardAction { get; set; }

    private static readonly string[] DestructiveKeywords = { "rm", "mkdir", "touch", "mv", "cp", "chmod", "chown", "git commit", "git push" };

    public MainWindowViewModel()
    {
        _terminalService = new TerminalService();
        _aiInferenceService = new AiInferenceService();
        
        ChatTimeline.Add(new AiChatMessage { Message = "Hello! I am PromptShell. How can I assist you with your workspace today?" });
    }

    [RelayCommand]
    private async Task ExecuteTerminalCommandAsync()
    {
        if (string.IsNullOrWhiteSpace(InputCommand) || IsPendingApproval)
            return;

        string userRequest = InputCommand;
        _history.Add(userRequest);
        _historyIndex = _history.Count;
        
        ChatTimeline.Add(new UserChatMessage { Message = userRequest });
        InputCommand = string.Empty;

        try
        {
            string directoryContext = "Empty folder";
            if (Directory.Exists(CurrentWorkingDirectory))
            {
                string[] excludedFolders = { "node_modules", "bin", "obj", ".git", ".idea", ".vs", "dist", "out" };
                var allowedFiles = Directory.GetFiles(CurrentWorkingDirectory).Select(Path.GetFileName)
                    .Where(file => file != null && !file.StartsWith(".") && !file.EndsWith(".png") && !file.EndsWith(".jpg") && !file.EndsWith(".exe") && !file.EndsWith(".dll"));
                var allowedDirs = Directory.GetDirectories(CurrentWorkingDirectory).Select(Path.GetFileName)
                    .Where(dir => dir != null && !dir.StartsWith(".") && !excludedFolders.Contains(dir));
                
                var finalContextList = allowedDirs.Concat(allowedFiles).ToList();
                if (finalContextList.Any()) directoryContext = string.Join(", ", finalContextList);
            }
            
            string finalPromptToSend = userRequest;
            if (!string.IsNullOrWhiteSpace(_lastAiQuestion))
            {
                finalPromptToSend = $"Context of ongoing conversation:\nAI previously asked: '{_lastAiQuestion}'\nUser replied: '{userRequest}'\n\nConvert into final command.";
            }

            string aiResponse = await _aiInferenceService.GenerateCommandAsync(finalPromptToSend, directoryContext, Settings);

            if (string.IsNullOrWhiteSpace(aiResponse) || aiResponse.StartsWith("# Error"))
            {
                ChatTimeline.Add(new AiChatMessage { Message = aiResponse.StartsWith("# Error") ? aiResponse : "Sorry, I couldn't internalize that request." });
                return;
            }
            
            if (aiResponse.StartsWith("?"))
            {
                _lastAiQuestion = aiResponse.TrimStart('?');
                ChatTimeline.Add(new AiChatMessage { Message = _lastAiQuestion });
                LogLatestSession(directoryContext, userRequest, "N/A", _lastAiQuestion);
                return;
            }

            string activeQuestionContext = _lastAiQuestion;
            _lastAiQuestion = string.Empty;
            
            if (RequiresApproval(aiResponse))
            {
                PendingCommand = aiResponse;
                IsPendingApproval = true;
                
                ChatTimeline.Add(new ActionCardChatMessage 
                { 
                    Message = "PromptShell wants to execute a modification command.",
                    PendingCommand = aiResponse
                });
                LogLatestSession(directoryContext, userRequest, aiResponse, "Pending user approval...");
            }
            else
            {
                await RunAndAnalyzeCommandAsync(aiResponse, directoryContext, userRequest, activeQuestionContext);
            }
        }
        catch (Exception ex)
        {
            ChatTimeline.Add(new SystemChatMessage { Message = $"Fatal Error: {ex.Message}" });
        }
    }

    private async Task RunAndAnalyzeCommandAsync(string command, string directoryContext, string userRequest, string activeQuestionContext)
    {
        var result = await _terminalService.ExecuteCommandAsync(command, CurrentWorkingDirectory);
        string explanation = await _aiInferenceService.InterpretResultAsync(command, result, Settings);
        
        TerminalOutput = result.IsSuccessful ? result.Output : result.Error;
        AiExplanation = explanation;
        
        ChatTimeline.Add(new AiChatMessage
        {
            Message = explanation,
            RawTechnicalDetails = $"[Executed Command]: {command}\n\n[Exit Code]: {result.ExitCode}\n\n[Console Output]:\n{(result.IsSuccessful ? result.Output : result.Error)}"
        });

        AppendToCommandLog(command, result.ExitCode, result.IsSuccessful);
        LogLatestSession(directoryContext, userRequest, command, explanation, result.Output, result.Error, result.ExitCode);
    }

    [RelayCommand]
    public async Task ApproveCommandAsync()
    {
        if (string.IsNullOrWhiteSpace(PendingCommand)) return;
        
        var card = ChatTimeline.LastOrDefault(m => m is ActionCardChatMessage) as ActionCardChatMessage;
        if (card != null) card.IsActionPending = false;

        string cmdToRun = PendingCommand;
        IsPendingApproval = false;
        PendingCommand = string.Empty;
        
        await RunAndAnalyzeCommandAsync(cmdToRun, "Cached on approval", "Approved command execution", "");
    }

    [RelayCommand]
    public void RejectCommand()
    {
        var card = ChatTimeline.LastOrDefault(m => m is ActionCardChatMessage) as ActionCardChatMessage;
        if (card != null) card.IsActionPending = false;

        AppendToCommandLog($"REJECTED: {PendingCommand}", -1, false);
        PendingCommand = string.Empty;
        IsPendingApproval = false;

        ChatTimeline.Add(new SystemChatMessage { Message = "The proposed command has been rejected by the user." });
    }

    private void AppendToCommandLog(string command, int exitCode, bool isSuccess)
    {
        try
        {
            if (File.Exists(CommandsLogPath))
            {
                var fileInfo = new FileInfo(CommandsLogPath);
                if (fileInfo.Length > 5 * 1024 * 1024) File.WriteAllText(CommandsLogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Log cleared automatically.{Environment.NewLine}", Encoding.UTF8);
            }
            File.AppendAllText(CommandsLogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Status: {(isSuccess ? "SUCCESS" : "FAILED")} | Command: {command}{Environment.NewLine}", Encoding.UTF8);
        }
        catch { /* ignore */ }
    }

    private void LogLatestSession(string context, string userRequest, string generatedCommand, string aiExplanation, string stdout = "", string stderr = "", int? exitCode = null)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"PROMPTSHELL LOG - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Directory: {context}\nRequest: {userRequest}\nCommand: {generatedCommand}\nAI Analysis: {aiExplanation}");
            File.WriteAllText(SessionLogPath, sb.ToString(), Encoding.UTF8);
        }
        catch { /* ignore */ }
    }

    [RelayCommand] public void NavigateHistoryUp() { if (_history.Count == 0 || _historyIndex <= 0) return; _historyIndex--; InputCommand = _history[_historyIndex]; }
    [RelayCommand] public void NavigateHistoryDown() { if (_historyIndex < _history.Count - 1) { _historyIndex++; InputCommand = _history[_historyIndex]; } else { _historyIndex = _history.Count; InputCommand = string.Empty; } }
    [RelayCommand] 
    private void ClearConsole() 
    { 
        ChatTimeline.Clear(); 
        ChatTimeline.Add(new AiChatMessage { Message = "Chat timeline cleared. How can I help you now?" }); 
    }
    [RelayCommand] private async Task CopyExplanationToClipboard() { if (!string.IsNullOrWhiteSpace(AiExplanation) && CopyToClipboardAction != null) await CopyToClipboardAction(AiExplanation); }
    private bool RequiresApproval(string command) { var tokens = command.Split(new[] { ' ', '"', '\'' }, StringSplitOptions.RemoveEmptyEntries); return tokens.Any(token => DestructiveKeywords.Contains(token)) || command.Contains(">") || command.Contains(">>"); }
}