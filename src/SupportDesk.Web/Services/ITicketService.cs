using SupportDesk.Web.Models;

namespace SupportDesk.Web.Services;

public interface ITicketService
{
    Task<Ticket[]> GetAllAsync(TicketStatus? status = null, CancellationToken ct = default);
    Task<Ticket?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<Ticket> CreateAsync(string title, string description, int priority, CancellationToken ct = default);
    Task<StatusChangeResult> ChangeStatusAsync(int id, TicketStatus newStatus, CancellationToken ct = default);
    Task<(AddCommentResult Result, Comment? Comment)> AddCommentAsync(int ticketId, string author, string text, CancellationToken ct = default);
    Task<Comment[]> GetCommentsAsync(int ticketId, CancellationToken ct = default);
}

public enum StatusChangeResult
{
    Changed,
    NotFound,
    NotAllowed
}

public enum AddCommentResult
{
    Added,
    TicketNotFound,
    TicketResolved
}
