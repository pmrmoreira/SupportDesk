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

    // Aula 16, exercício C: o contexto que pedimos ao Ollama (num_ctx), em tokens. 4096 é o valor por omissão dele.
    [Range(2048, 131072)]
    public int ContextTokens { get; set; } = 4096;
}
