namespace PromptShell.Models;

public class AppSettings
{
    // possible values: "Proxy", "CustomApi", "Ollama", "OfflineOnnx"
    public string AiProvider { get; set; } = "Proxy";
    public string CustomApiKey { get; set; } = string.Empty;
    public string CustomApiUrl { get; set; } = "https://api.groq.com/openai/v1/chat/completions";
    public string CustomModelName { get; set; } = "llama3-8b-8192";
    public bool IsOfflineModelDownloaded { get; set; } = false;
}