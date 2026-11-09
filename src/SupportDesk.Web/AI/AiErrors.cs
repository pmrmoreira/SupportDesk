using System.Net;
using System.Text.Json;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace SupportDesk.Web.Ai;

// As falhas esperadas de uma chamada ao modelo, em mensagens para quem está a usar a página.
public static class AiErrors
{
    public static bool IsExpected(Exception ex) =>
        ex is HttpRequestException or JsonException
        // Aula 21: com o pipeline de resiliência, o tempo esgotado e o circuito aberto chegam assim.
        or TimeoutRejectedException or BrokenCircuitException or AiDisabledException
        || ex is TaskCanceledException { InnerException: TimeoutException };

    public static string Describe(Exception ex, OllamaOptions options) => Describe(ex, options, options.ChatModel);

    // Aula 18: há dois modelos (chat e embeddings); a mensagem do 404 diz qual faltou.
    public static string Describe(Exception ex, OllamaOptions options, string model) => ex switch
    {
        TimeoutRejectedException or TaskCanceledException =>
            $"O Ollama não respondeu em {options.TimeoutSeconds} s. A primeira chamada carrega o modelo e pode demorar: tenta outra vez.",
        BrokenCircuitException =>
            "O Ollama falhou várias vezes seguidas: a aplicação deixou de lhe fazer pedidos durante uns segundos. Tenta daqui a pouco.",
        AiDisabledException =>
            "A IA está desligada nesta instalação (AI:Ollama:Enabled = false).",
        HttpRequestException { StatusCode: HttpStatusCode.NotFound } =>
            $"O modelo «{model}» não está instalado no Ollama. Corre: ollama pull {model}",
        HttpRequestException { HttpRequestError: HttpRequestError.ConnectionError } =>
            $"Não foi possível ligar ao Ollama em {options.Url}. Confirma que está a correr.",
        HttpRequestException http =>
            $"O Ollama respondeu com um erro ({(int?)http.StatusCode}).",
        JsonException =>
            "A IA devolveu uma resposta que não se consegue ler.",
        _ => "A chamada à IA falhou."
    };
}
