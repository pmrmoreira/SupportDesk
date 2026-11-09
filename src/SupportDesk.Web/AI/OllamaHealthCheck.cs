using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OllamaSharp;

namespace SupportDesk.Web.Ai;

// Aula 21: o Ollama responde, e tem os dois modelos? (GET /api/tags, pelo mesmo HttpClient da aplicação.)
public class OllamaHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClients;
    private readonly OllamaOptions _options;

    public OllamaHealthCheck(IHttpClientFactory httpClients, IOptions<OllamaOptions> options)
    {
        _httpClients = httpClients;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return HealthCheckResult.Healthy("IA desligada por configuração.");
        }
        try
        {
            var ollama = new OllamaApiClient(_httpClients.CreateClient("Ollama"));
            var installed = (await ollama.ListLocalModelsAsync(cancellationToken)).Select(m => m.Name).ToHashSet();
            // O Ollama mostra «bge-m3:latest» para um modelo descarregado como «bge-m3».
            var missing = new[] { _options.ChatModel, _options.EmbeddingModel }
                .Where(model => !installed.Contains(model) && !installed.Contains($"{model}:latest"))
                .ToList();
            return missing.Count == 0
                ? HealthCheckResult.Healthy($"Modelos instalados: {_options.ChatModel}, {_options.EmbeddingModel}.")
                : HealthCheckResult.Degraded($"Faltam modelos no Ollama: {string.Join(", ", missing)}.");
        }
        catch (Exception ex) when (AiErrors.IsExpected(ex))
        {
            return HealthCheckResult.Unhealthy(AiErrors.Describe(ex, _options));
        }
    }
}
