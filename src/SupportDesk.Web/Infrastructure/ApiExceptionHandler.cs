using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace SupportDesk.Web.Infrastructure;

// Erros inesperados na API: responde com ProblemDetails, sem detalhes internos (aula 7, ciclo 3).
public class ApiExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(IProblemDetailsService problemDetails, ILogger<ApiExceptionHandler> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        // Aqui, Request.Path já aponta para /Error; o caminho original está nesta feature.
        var path = httpContext.Features.Get<IExceptionHandlerPathFeature>()?.Path ?? "";
        if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            return false; // páginas: fica para a página /Error
        }

        // No .NET 10, se o handler trata a exceção, o middleware já não a regista: regista-a aqui.
        _logger.LogError(exception, "Erro em {Method} {Path}", httpContext.Request.Method, path);

        // O EF Core embrulha as falhas de ligação numa InvalidOperationException.
        var databaseDown = exception is NpgsqlException { IsTransient: true }
            || exception.InnerException is NpgsqlException { IsTransient: true };

        var (status, title) = databaseDown
            ? (StatusCodes.Status503ServiceUnavailable, "Base de dados indisponível")
            : (StatusCodes.Status500InternalServerError, "Erro inesperado");

        httpContext.Response.StatusCode = status;
        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = "Tenta outra vez daqui a pouco. Se o erro continuar, avisa o suporte."
            }
        });
    }
}
