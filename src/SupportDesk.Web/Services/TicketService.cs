using SupportDesk.Web.Models;

namespace SupportDesk.Web.Services;

public class TicketService : ITicketService
{
    private readonly List<Ticket> _tickets =
    [
        new Ticket {Id = 1, Title = "Não consigo entrar", Status = TicketStatus.Open},
        new Ticket {Id = 2, Title = "Fatura com valor incorreto", Status = TicketStatus.InProgress},
        new Ticket {Id = 3, Title = "Impressora indisponível", Status = TicketStatus.Open}
    ];
    private readonly Lock _lock = new();
    private int _nextId = 4;

    private readonly List<Comment> _comments = [];
    private int _nextCommentId = 1;




    public Ticket[] GetAll(TicketStatus? status = null)
    {
        lock (_lock)
        {
            return _tickets.Where(t=> status is null || t.Status == status).ToArray();
        }
    }
    public Ticket? GetById(int id)
    {
        lock (_lock)
        {
            return _tickets.FirstOrDefault(t => t.Id == id);
        }
    }
    public int Count()
    {
        lock( _lock)
        {
            return _tickets.Count;
        }
    }
    public Ticket Create(string title, string description)
    {
        lock(_lock)
        {
            var ticket = new Ticket
            {
                Id = _nextId++,
                Title = title,
                Description = description,
                Status = TicketStatus.Open
            };
            _tickets.Add(ticket);
            return ticket;
        }
    }
    public StatusChangeResult ChangeStatus(int id, TicketStatus newStatus)
    {
        lock (_lock)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == id);
            if (ticket == null)
            {
                return StatusChangeResult.NotFound;
            }
            if (ticket.Status.Equals(newStatus))
            {
                return StatusChangeResult.Changed;
            }
            if (!IsAllowed(ticket.Status, newStatus))
            {
                return StatusChangeResult.NotAllowed;
            }
            ticket.Status = newStatus;
            return StatusChangeResult.Changed;
        }
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
    public (AddCommentResult Result, Comment? Comment) AddComment(int ticketId, string author, string text)
    {
        lock (_lock)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket is null)
            {
                return (AddCommentResult.TicketNotFound, null);
            }
            if (ticket.Status == TicketStatus.Resolved)
            {
                return (AddCommentResult.TicketResolved, null);
            }
            var comment = new Comment
            {
                Id = _nextCommentId++,
                TicketId = ticketId,
                Author = author,
                Text = text,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _comments.Add(comment);
            return (AddCommentResult.Added, comment);
        }
    }
    public Comment[] GetComments(int ticketId)
    {
        lock (_lock)
        {
            return _comments.Where(c  => c.TicketId == ticketId).ToArray();
        }
    }
    public enum AddCommentResult
    {
        Added,
        TicketNotFound,
        TicketResolved
    }

}
