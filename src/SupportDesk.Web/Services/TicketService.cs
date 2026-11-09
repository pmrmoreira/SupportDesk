using Microsoft.EntityFrameworkCore;
using SupportDesk.Web.Contracts;
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

    public async Task<PagedResult<TicketListItem>> SearchAsync(TicketQuery query, string? ownerId, CancellationToken ct = default)
    {
        var tickets = _db.Tickets.AsNoTracking();

        if (ownerId is not null)
        {
            tickets = tickets.Where(t => t.CreatedById == ownerId);
        }

        if (query.Status is not null)
        {
            tickets = tickets.Where(t => t.Status == query.Status);
        }
        if (query.Priority is not null)
        {
            tickets = tickets.Where(t => t.Priority == query.Priority);
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Exercício C: % e _ do utilizador passam a ser texto normal.
            // O terceiro argumento (@"\") diz ao PostgreSQL qual é o carácter de escape.
            var pattern = $"%{EscapeLike(query.Search.Trim())}%";
            tickets = tickets.Where(t =>
                EF.Functions.ILike(t.Title, pattern, @"\") || EF.Functions.ILike(t.Description, pattern, @"\"));
        }

        var totalCount = await tickets.CountAsync(ct);

        // Exercício A: cada ordem acaba no Id, para a ordem ser sempre a mesma entre páginas.
        tickets = query.Sort switch
        {
            TicketSort.Oldest => tickets.OrderBy(t => t.CreatedAt).ThenBy(t => t.Id),
            TicketSort.Priority => tickets.OrderBy(t => t.Priority).ThenByDescending(t => t.CreatedAt).ThenByDescending(t => t.Id),
            TicketSort.Activity => tickets.OrderByDescending(t => t.UpdatedAt).ThenByDescending(t => t.Id),
            _ => tickets.OrderByDescending(t => t.CreatedAt).ThenByDescending(t => t.Id)
        };

        var items = await tickets
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new TicketListItem(
                t.Id, t.Title, t.Status, t.Priority, t.CreatedAt, t.UpdatedAt,
                _db.Comments.Count(c => c.TicketId == t.Id)))
            .ToListAsync(ct);

        return new PagedResult<TicketListItem>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<TicketDetails?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Tickets
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TicketDetails(
                t.Id, t.Title, t.Description, t.Status, t.Priority, t.CreatedAt, t.UpdatedAt, t.Version,
                t.CreatedById,
                // Aula 10: o EF Core traduz a navegação num LEFT JOIN com AspNetUsers, na mesma consulta.
                t.CreatedBy == null ? null
                    : t.CreatedBy.DisplayName == "" ? t.CreatedBy.Email : t.CreatedBy.DisplayName,
                t.Category))   // aula 15
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CountAsync(string? ownerId, CancellationToken ct = default)
    {
        return await _db.Tickets.CountAsync(t => ownerId == null || t.CreatedById == ownerId, ct);
    }

    // Exercício B da aula 6: GROUP BY na base; os estados sem pedidos ficam com 0.
    public async Task<Dictionary<TicketStatus, int>> CountByStatusAsync(string? ownerId, CancellationToken ct = default)
    {
        var counts = await _db.Tickets
            .Where(t => ownerId == null || t.CreatedById == ownerId)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Total = g.Count() })
            .ToListAsync(ct);

        return Enum.GetValues<TicketStatus>().ToDictionary(
            status => status,
            status => counts.FirstOrDefault(c => c.Status == status)?.Total ?? 0);
    }

    // Devolve o DTO (antes devolvia Task<Ticket>, que não batia com a interface: erro de compilação).
    public async Task<TicketDetails> CreateAsync(string title, string description, int priority, string createdById, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var ticket = new Ticket
        {
            Title = title,
            Description = description,
            Priority = priority,
            Status = TicketStatus.Open,
            CreatedAt = now,
            UpdatedAt = now,    // exercício C da aula 6
            CreatedById = createdById
        };
        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync(ct);
        return ToDetails(ticket, await DisplayNameAsync(createdById, ct));
    }

    public async Task<UpdateTicketResult> UpdateAsync(int id, string title, string description, int priority, uint version, CancellationToken ct = default)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (ticket is null)
        {
            return UpdateTicketResult.NotFound;
        }
        if (ticket.Status == TicketStatus.Resolved)
        {
            return UpdateTicketResult.TicketResolved;
        }

        // O UPDATE compara o xmin com a versão que o formulário trouxe, não com a que acabámos de ler:
        // assim apanha também quem gravou enquanto a página estava aberta.
        _db.Entry(ticket).Property(t => t.Version).OriginalValue = version;
        ticket.Title = title;
        ticket.Description = description;
        ticket.Priority = priority;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return UpdateTicketResult.Conflict;
        }
        return UpdateTicketResult.Updated;
    }

    public async Task<StatusChangeResult> ChangeStatusAsync(int id, TicketStatus newStatus, CancellationToken ct = default)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (ticket is null)
        {
            return StatusChangeResult.NotFound;
        }
        if (ticket.Status == newStatus)
        {
            return StatusChangeResult.Changed; // nada para mudar
        }
        if (!TicketWorkflow.CanChange(ticket.Status, newStatus))
        {
            return StatusChangeResult.NotAllowed;
        }


        var oldStatus = ticket.Status;
        ticket.Status = newStatus;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        _db.Comments.Add(NewComment(id, "Sistema", $"Estado alterado de {oldStatus} para {newStatus}."));

        // Concorrência otimista: o UPDATE leva "AND xmin = ..."; se outra pessoa gravou
        // entretanto, afeta 0 linhas e o EF lança DbUpdateConcurrencyException.
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return StatusChangeResult.Conflict;
        }
        return StatusChangeResult.Changed;
    }

    // Devolve CommentResponse? no tuplo (antes era Comment?: outro erro de compilação).
    public async Task<(AddCommentResult Result, CommentResponse? Comment)> AddCommentAsync(int ticketId, string authorId, string text, CancellationToken ct = default)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId, ct);
        if (ticket is null)
        {
            return (AddCommentResult.TicketNotFound, null);
        }
        if (ticket.Status == TicketStatus.Resolved)
        {
            return (AddCommentResult.TicketResolved, null);
        }

        // Aula 10: ficam as duas coisas, quem foi (AuthorId) e o nome nesse momento (Author).
        var author = await DisplayNameAsync(authorId, ct) ?? "?";
        var comment = NewComment(ticketId, author, text);
        comment.AuthorId = authorId;
        _db.Comments.Add(comment);
        ticket.UpdatedAt = comment.CreatedAt;   // comentar também conta como atividade

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return (AddCommentResult.Conflict, null);
        }
        return (AddCommentResult.Added, ToResponse(comment));
    }

    public async Task<CommentResponse[]> GetCommentsAsync(int ticketId, CancellationToken ct = default)
    {
        return await _db.Comments
            .AsNoTracking()
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Select(c => new CommentResponse(c.Id, c.Author, c.Text, c.CreatedAt))
            .ToArrayAsync(ct);
    }

    private static Comment NewComment(int ticketId, string author, string text) => new()
    {
        TicketId = ticketId,
        Author = author,
        Text = text,
        CreatedAt = DateTimeOffset.UtcNow
    };

    // Em LIKE/ILIKE, % e _ são curingas. Com \ antes, passam a ser texto.
    // A \ troca-se primeiro, senão as \ acabadas de pôr eram duplicadas.
    private static string EscapeLike(string text) =>
        text.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");

    // O nome que aparece nas páginas: o DisplayName, ou o email para quem se registou pela API.
    private async Task<string?> DisplayNameAsync(string userId, CancellationToken ct) =>
        await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.DisplayName == "" ? u.Email : u.DisplayName)
            .FirstOrDefaultAsync(ct);

    // Aula 15: um pedido acabado de criar ainda não tem categoria.
    private static TicketDetails ToDetails(Ticket t, string? createdByName) =>
        new(t.Id, t.Title, t.Description, t.Status, t.Priority, t.CreatedAt, t.UpdatedAt, t.Version,
            t.CreatedById, createdByName, Category: null);

    private static CommentResponse ToResponse(Comment c) =>
        new(c.Id, c.Author, c.Text, c.CreatedAt);
}
