using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Web.Ai;

public class OllamaOptions
{
    public const string Section = "AI:Ollama";

    // Aula 21: false desliga a IA sem mudar código; o resto da aplicação continua a funcionar.
    public bool Enabled { get; set; } = true;

    [Required, Url]
    public string Url { get; set; } = "http://localhost:11434";

    [Required]
    public string ChatModel { get; set; } = "gemma3:4b";

    // Aula 18: o modelo que transforma texto em vetores (embeddings).
    [Required]
    public string EmbeddingModel { get; set; } = "bge-m3";

    // A primeira chamada carrega o modelo em memória e pode demorar.
    // Aula 21: é o tempo total de uma chamada, com as repetições incluídas.
    [Range(5, 600)]
    public int TimeoutSeconds { get; set; } = 120;

    // Aula 21: pedidos à IA por utilizador e por minuto.
    [Range(1, 1000)]
    public int RequestsPerMinute { get; set; } = 10;

    // Aula 16, exercício C: o contexto que pedimos ao Ollama (num_ctx), em tokens. 4096 é o valor por omissão dele.
    [Range(2048, 131072)]
    public int ContextTokens { get; set; } = 4096;
}
