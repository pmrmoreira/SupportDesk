using SupportDesk.Web.Ai;
using SupportDesk.Web.Contracts;

namespace SupportDesk.Web.Services;

// As sugestões da IA de cada pedido: gravar, ler a mais recente e decidir.
public interface ISuggestionService
{
    Task<SuggestionResponse> AddAsync(int ticketId, TicketClassification classification, ClassificationResult call, CancellationToken ct = default);
    Task<SuggestionResponse?> GetLatestAsync(int ticketId, CancellationToken ct = default);
    Task<DecideResult> DecideAsync(int ticketId, int suggestionId, bool accept, string userId, CancellationToken ct = default);
}

public enum DecideResult
{
    Applied,
    NotFound,
    AlreadyDecided,
    Conflict
}
