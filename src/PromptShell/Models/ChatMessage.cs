using System;

namespace PromptShell.Models;

public class ChatMessage
{
    // "User", "AI", "System", "ActionCard"
    public string Sender { get; set; } = "User";
    public string Message { get; set; } = string.Empty;
    public string Timestamp { get; set; } = DateTime.Now.ToString("HH:mm");
    
    public string PendingCommand { get; set; } = string.Empty;
    public bool IsActionPending { get; set; } = false;
    
    public string RawTechnicalDetails { get; set; } = string.Empty;
    public bool HasTechnicalDetails => !string.IsNullOrWhiteSpace(RawTechnicalDetails);
}