using Microsoft.EntityFrameworkCore;
using SupportDesk.Web.Data;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Services;

public class TicketService : ITicketService
{

    private readonly SupportDeskDbContext _db;

    public TicketService(SupportDeskDbContext db)
    {
        _db = db;
    }



    public async Task<Ticket[]> GetAllAsync(TicketStatus? status = null, CancellationToken ct = default)
    {
        return await _db.Tickets
            .Where(t => status == null || t.Status == status)
            .OrderBy(t => t.Id)
            .ToArrayAsync(ct);
    }
    public async Task<Ticket?> GetByIdAsycn(int id, CancellationToken ct = default)
    {
        return await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id,ct);
    }
    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _db.Tickets.CountAsync(ct);
    }
    public async Task<Ticket> Create(string title, string description, int priority, CancellationToken ct = default)
    {
        var ticket = new Ticket
        {
            Title = title,
            Description = description,
            Priority = priority,
            Status = TicketStatus.Open,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync(ct);
        return ticket;
    }
    public async Task<StatusChangeResult> ChangeStatusAsync(int id, TicketStatus newStatus, CancellationToken ct = default)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t =>t.Id == id, ct);
        if (ticket == null)
        {
            return StatusChangeResult.NotFound;
        }
        if (ticket.Status == newStatus)
        {
            return StatusChangeResult.Changed;
        }
        if (!IsAllowed(ticket.Status, newStatus))
        {
            return StatusChangeResult.NotAllowed;
        }
        var oldStatus = ticket.Status;
        ticket.Status = newStatus;
        _db.Comments.Add(NewComment(id, "Sistema", $"Estado alterado de {oldStatus} para {newStatus}."));
        await _db.SaveChangesAsync(ct);
        return StatusChangeResult.Changed;
            
    }
    private static bool IsAllowed(TicketStatus from, TicketStatus to) => (from, to) switch
    {
        (TicketStatus.Open, TicketStatus.InProgress) => true,
        (TicketStatus.InProgress, TicketStatus.Resolved) => true,
        (TicketStatus.InProgress, TicketStatus.Open) => true,
        (TicketStatus.Resolved, TicketStatus.Open) => true,
        _ => false
    };


    public enum StatusChangeResult
    {
        Changed,
        NotFound,
        NotAllowed
    }
    public async Task<(AddCommentResult Result, Comment? Comment)> AddCommentAsync(int ticketId, string author, string text, CancellationToken ct = default)
    {
        
        
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId,ct);
        if (ticket is null)
        {
            return (AddCommentResult.TicketNotFound, null);
        }
        if (ticket.Status == TicketStatus.Resolved)
        {
            return (AddCommentResult.TicketResolved, null);
        }
        var comment = NewComment(ticketId, author, text);
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync(ct);
        return (AddCommentResult.Added, comment);
        
    }
    public async Task<Comment[]> GetCommentsAsync(int ticketId, CancellationToken ct = default)
    {
        return await _db.Comments
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .ToArrayAsync(ct);
    }
    private static Comment NewComment(int ticketId, string author, string text) => new()
    {
        TicketId = ticketId,
        Author = author,
        Text = text,
        CreatedAt = DateTimeOffset.UtcNow
    };
    public enum AddCommentResult
    {
        Added,
        TicketNotFound,
        TicketResolved
    }

}
