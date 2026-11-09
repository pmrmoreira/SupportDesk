using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Web.Ai;


public class OllamaOptions
{
    public const string Section = "AI:Ollama";

    [Required, Url]
    public string Url { get; set; } = "http://localhost:11434";

    [Required]
    public string ChatModel { get; set; } = "gemma3:4b";

    
    [Range(5, 600)]
    public int TimeoutSeconds { get; set; } = 120;
}
