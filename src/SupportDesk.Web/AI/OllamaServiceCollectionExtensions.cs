using Microsoft.Extensions.AI;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using OllamaSharp;
using Polly;

namespace SupportDesk.Web.Ai;

// Aula 20: o registo da IA num só sítio, usado pela aplicação (Program.cs) e pela ferramenta de avaliação.
public static class OllamaServiceCollectionExtensions
{
    // Exercício C da aula 21: o nome da fonte dos traços (Activity) das chamadas à IA.
    public const string TelemetrySource = "SupportDesk.Ai";

    public static IServiceCollection AddOllamaAi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OllamaOptions>()
            .Bind(configuration.GetSection(OllamaOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddHttpClient("Ollama", (provider, http) =>
            {
                var options = provider.GetRequiredService<IOptions<OllamaOptions>>().Value;
                http.BaseAddress = new Uri(options.Url);
                // Aula 21: quem decide o tempo é o pipeline de resiliência (abaixo), não o HttpClient.
                http.Timeout = Timeout.InfiniteTimeSpan;
            })
            // Aula 21: cada pedido ao Ollama passa por estas três estratégias, de fora para dentro.
            .AddResilienceHandler("ollama", (pipeline, context) =>
            {
                var options = context.ServiceProvider.GetRequiredService<IOptions<OllamaOptions>>().Value;
                // 1. O tempo total de uma chamada, com as repetições incluídas.
                pipeline.AddTimeout(TimeSpan.FromSeconds(options.TimeoutSeconds));
                // 2. Repetir o que é passageiro (500 ou mais, 408, 429, ligação recusada): mais duas vezes, com espera crescente.
                pipeline.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 2,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                });
                // 3. Se metade dos pedidos falhar (com pelo menos 4 em 30 s), parar de pedir durante 30 s.
                pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 4,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    BreakDuration = TimeSpan.FromSeconds(30)
                });
            });

        // Aula 21: IA desligada → o mesmo cliente «desligado» no lugar dos dois do Ollama.
        if (!configuration.GetValue($"{OllamaOptions.Section}:Enabled", true))
        {
            services.AddScoped<IChatClient, DisabledAiClient>();
            services.AddScoped<IEmbeddingGenerator<string, Embedding<float>>, DisabledAiClient>();
            return services;
        }

        services.AddChatClient(provider =>
            {
                var options = provider.GetRequiredService<IOptions<OllamaOptions>>().Value;
                var http = provider.GetRequiredService<IHttpClientFactory>().CreateClient("Ollama");
                return new OllamaApiClient(http, options.ChatModel);
            }, ServiceLifetime.Scoped)
            .UseLogging()
            // Exercício C: um traço por chamada, com as convenções gen_ai do OpenTelemetry (modelo, tokens, tempo).
            .UseOpenTelemetry(sourceName: TelemetrySource);
        // Aula 18: embeddings com o mesmo Ollama, outro modelo (bge-m3), por trás de IEmbeddingGenerator.
        services.AddEmbeddingGenerator(provider =>
            {
                var options = provider.GetRequiredService<IOptions<OllamaOptions>>().Value;
                var http = provider.GetRequiredService<IHttpClientFactory>().CreateClient("Ollama");
                return new OllamaApiClient(http, options.EmbeddingModel);
            }, ServiceLifetime.Scoped)
            .UseLogging()
            .UseOpenTelemetry(sourceName: TelemetrySource);
        return services;
    }
}
