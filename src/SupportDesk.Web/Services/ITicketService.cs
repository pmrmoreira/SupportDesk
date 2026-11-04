using SupportDesk.Web.Contracts;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Services;

public interface ITicketService
{
    // ownerId: null = todos os pedidos (técnico); um id = só os pedidos desse cliente.
    Task<PagedResult<TicketListItem>> SearchAsync(TicketQuery query, string? ownerId, CancellationToken ct = default);
    Task<TicketDetails?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CountAsync(string? ownerId, CancellationToken ct = default);
    Task<Dictionary<TicketStatus, int>> CountByStatusAsync(string? ownerId, CancellationToken ct = default);
    
    Task<TicketDetails> CreateAsync(string title, string description, int priority, string createdById, CancellationToken ct = default);
    // Aula 9: version é a que o formulário mostrou; se alguém gravou entretanto, dá Conflict.
    Task<UpdateTicketResult> UpdateAsync(int id, string title, string description, int priority, uint version, CancellationToken ct = default);
    Task<StatusChangeResult> ChangeStatusAsync(int id, TicketStatus newStatus, CancellationToken ct = default);
    Task<(AddCommentResult Result, CommentResponse? Comment)> AddCommentAsync(int ticketId, string authorId, string text, CancellationToken ct = default);
    Task<CommentResponse[]> GetCommentsAsync(int ticketId, CancellationToken ct = default);

}

public enum StatusChangeResult
{
    Changed,
    NotFound,
    NotAllowed,
    Conflict      // aula 7, ciclo 3: outra pessoa mudou o pedido ao mesmo tempo
}

public enum UpdateTicketResult
{
    Updated,
    NotFound,
    TicketResolved,
    Conflict
}

public enum AddCommentResult
{
    Added,
    TicketNotFound,
    TicketResolved,
    Conflict
}
