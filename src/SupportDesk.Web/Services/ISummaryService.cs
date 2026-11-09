using SupportDesk.Web.Ai;
using SupportDesk.Web.Contracts;

namespace SupportDesk.Web.Services;

// O último resumo de cada pedido: ler e substituir.
public interface ISummaryService
{
    Task<SummaryResponse?> GetAsync(int ticketId, CancellationToken ct = default);
    Task SaveAsync(int ticketId, TicketSummary summary, SummaryResult call, DateTimeOffset sourceUpdatedAt, CancellationToken ct = default);
}
