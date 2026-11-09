using Microsoft.Extensions.AI;

namespace SupportDesk.Web.Ai;

// Aula 21: com a IA desligada, todas as chamadas falham da mesma maneira, e o AiErrors explica porquê.
public sealed class AiDisabledException() : Exception("A IA está desligada.");

// Fica no lugar do OllamaApiClient quando AI:Ollama:Enabled = false. Ninguém mais precisa de saber.
public sealed class DisabledAiClient : IChatClient, IEmbeddingGenerator<string, Embedding<float>>
{
    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new AiDisabledException();

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new AiDisabledException();

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new AiDisabledException();

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
