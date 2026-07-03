using System;

namespace PromptShell.Models;

public class ChatMessage
{
    public string Message { get; set; } = string.Empty;
    public string Timestamp { get; set; } = DateTime.Now.ToString("HH:mm");
}

public class UserChatMessage : ChatMessage { }

public class AiChatMessage : ChatMessage 
{ 
    public string RawTechnicalDetails { get; set; } = string.Empty;
    public bool HasTechnicalDetails => !string.IsNullOrWhiteSpace(RawTechnicalDetails);
}

public class ActionCardChatMessage : ChatMessage
{
    public string PendingCommand { get; set; } = string.Empty;
    public bool IsActionPending { get; set; } = true;
}

public class SystemChatMessage : ChatMessage { }