using SupportDesk.Web.Ai;
using SupportDesk.Web.Contracts;

namespace SupportDesk.Web.Services;

// Os rascunhos de resposta de cada pedido: gravar, ler o mais recente, publicar ou descartar.
public interface IDraftService
{
    Task SaveAsync(int ticketId, DraftResult result, CancellationToken ct = default);
    Task<DraftResponse?> GetLatestAsync(int ticketId, CancellationToken ct = default);
    Task<DraftDecisionResult> PublishAsync(int ticketId, int draftId, string text, string userId, CancellationToken ct = default);
    Task<DraftDecisionResult> DiscardAsync(int ticketId, int draftId, string userId, CancellationToken ct = default);

    // Exercício C: quantos rascunhos foram usados, e quantos sem o técnico mexer no texto.
    Task<DraftStats> GetStatsAsync(CancellationToken ct = default);
}

public record DraftStats(int Total, int Published, int PublishedUnchanged, int Discarded, int Pending);

public enum DraftDecisionResult
{
    Done,
    NotFound,
    AlreadyDecided,
    Empty,
    InvalidCitations,
    TooLong,
    TicketResolved,
    Conflict
}
