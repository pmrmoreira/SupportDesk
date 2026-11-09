using System.Net;
using System.Text.Json;

namespace SupportDesk.Web.Ai;

 
public static class AiErrors
{
    public static bool IsExpected(Exception ex) =>
        ex is HttpRequestException or JsonException
        || ex is TaskCanceledException { InnerException: TimeoutException };

    public static string Describe(Exception ex, OllamaOptions options) => ex switch
    {
        TaskCanceledException =>
            $"O Ollama não respondeu em {options.TimeoutSeconds} s. A primeira chamada carrega o modelo e pode demorar: tenta outra vez.",
        HttpRequestException { StatusCode: HttpStatusCode.NotFound } =>
            $"O modelo «{options.ChatModel}» não está instalado no Ollama. Corre: ollama pull {options.ChatModel}",
        HttpRequestException { HttpRequestError: HttpRequestError.ConnectionError } =>
            $"Não foi possível ligar ao Ollama em {options.Url}. Confirma que está a correr.",
        HttpRequestException http =>
            $"O Ollama respondeu com um erro ({(int?)http.StatusCode}).",
        JsonException =>
            "A IA devolveu uma resposta que não se consegue ler.",
        _ => "A chamada à IA falhou."
    };
}
