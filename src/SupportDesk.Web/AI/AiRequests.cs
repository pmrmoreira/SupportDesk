namespace SupportDesk.Web.Ai;

// Aula 21: os pedidos HTTP que chamam um modelo, para os limitar por utilizador.
// (Nas Razor Pages, o [EnableRateLimiting] só se aplica à página inteira, não a um handler.)
public static class AiRequests
{
    // Os handlers que chamam o modelo: sugerir (15), resumir (16), gerar embeddings (18) e rascunhos (19).
    private static readonly HashSet<string> Handlers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Suggest", "Summarize", "Embed", "Generate"
    };

    public static bool IsAiRequest(HttpRequest request) =>
        // Os laboratórios (/Ai/…) quando se submete o formulário.
        (HttpMethods.IsPost(request.Method) && request.Path.StartsWithSegments("/Ai"))
        || (HttpMethods.IsPost(request.Method) && Handlers.Contains(request.Query["handler"].ToString()))
        // A pesquisa da base de conhecimento, que pede o embedding da pergunta (aula 18).
        || (HttpMethods.IsGet(request.Method) && request.Path.StartsWithSegments("/Knowledge")
            && !string.IsNullOrWhiteSpace(request.Query["q"]));

    // Cada pessoa tem a sua conta; sem sessão, conta o endereço IP.
    public static string UserKey(HttpContext context) =>
        context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anónimo";
}
