using SupportDesk.Web.Contracts;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Services;

public interface ITicketService
{
    Task<PagedResult<TicketListItem>> SearchAsync(TicketQuery query, CancellationToken ct = default);
    Task<TicketDetails?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<Dictionary<TicketStatus, int>> CountByStatusAsync(CancellationToken ct = default);
    Task<TicketDetails> CreateAsync(string title, string description, int priority, CancellationToken ct = default);
    Task<StatusChangeResult> ChangeStatusAsync(int id, TicketStatus newStatus, CancellationToken ct = default);
    Task<(AddCommentResult Result, CommentResponse? Comment)> AddCommentAsync(int ticketId, string author, string text, CancellationToken ct = default);
    Task<CommentResponse[]> GetCommentsAsync(int ticketId, CancellationToken ct = default);
}

public enum StatusChangeResult
{
    Changed,
    NotFound,
    NotAllowed,
    Conflict      // aula 7, ciclo 3: outra pessoa mudou o pedido ao mesmo tempo
}

public enum AddCommentResult
{
    Added,
    TicketNotFound,
    TicketResolved,
    Conflict
}
