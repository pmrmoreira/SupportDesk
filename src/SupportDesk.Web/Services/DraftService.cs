using Microsoft.EntityFrameworkCore;
using SupportDesk.Web.Ai;
using SupportDesk.Web.Contracts;
using SupportDesk.Web.Data;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Services;

public class DraftService : IDraftService
{
    // O limite de um comentário (AddCommentRequest).
    public const int MaxCommentLength = 2000;

    private readonly SupportDeskDbContext _db;
    private readonly ITicketService _tickets;

    public DraftService(SupportDeskDbContext db, ITicketService tickets)
    {
        _db = db;
        _tickets = tickets;
    }

    public async Task SaveAsync(int ticketId, DraftResult result, CancellationToken ct = default)
    {
        var draft = result.Draft ?? throw new ArgumentException("Só se gravam rascunhos com resposta.", nameof(result));
        _db.AiDrafts.Add(new TicketAiDraft
        {
            TicketId = ticketId,
            Reply = draft.Reply,
            NeedsClarification = draft.NeedsClarification,
            ClarifyingQuestion = draft.ClarifyingQuestion,
            InvalidCitations = result.Citations?.Invalid.Count ?? 0,
            Model = result.Model ?? "?",
            InputTokens = result.InputTokens,
            OutputTokens = result.OutputTokens,
            DurationMs = (int)result.Elapsed.TotalMilliseconds,
            CreatedAt = DateTimeOffset.UtcNow,
            Sources = result.Sources
                .Select(s => new TicketAiDraftSource { Number = s.Number, ArticleSlug = s.ArticleSlug, Heading = s.Heading })
                .ToList()
        });
        await _db.SaveChangesAsync(ct);
    }

    public Task<DraftResponse?> GetLatestAsync(int ticketId, CancellationToken ct = default) =>
        _db.AiDrafts
            .AsNoTracking()
            .Where(d => d.TicketId == ticketId)
            .OrderByDescending(d => d.Id)
            .Select(d => new DraftResponse(
                d.Id, d.Reply, d.NeedsClarification, d.ClarifyingQuestion, d.InvalidCitations, d.Model,
                d.InputTokens, d.OutputTokens, d.DurationMs, d.CreatedAt, d.Status,
                _db.Users.Where(u => u.Id == d.DecidedById)
                    .Select(u => u.DisplayName == "" ? u.Email : u.DisplayName)
                    .FirstOrDefault(),
                d.DecidedAt,
                d.Sources.OrderBy(s => s.Number)
                    .Select(s => new DraftSourceResponse(s.Number, s.ArticleSlug, s.Heading))
                    .ToList()))
            .FirstOrDefaultAsync(ct);

    // Publicar = um comentário público do técnico, com as fontes citadas no fim, e o rascunho fica decidido.
    public async Task<DraftDecisionResult> PublishAsync(
        int ticketId, int draftId, string text, string userId, CancellationToken ct = default)
    {
        var draft = await FindAsync(ticketId, draftId, ct);
        if (draft is null)
        {
            return DraftDecisionResult.NotFound;
        }
        if (draft.Status != DraftStatus.Pending)
        {
            return DraftDecisionResult.AlreadyDecided;
        }
        if (string.IsNullOrWhiteSpace(text))
        {
            return DraftDecisionResult.Empty;
        }

        // O técnico pode ter mexido nos números: confirma-se outra vez contra as fontes do rascunho.
        var citations = CitationCheck.Check(text.Trim(), draft.Sources.Count);
        if (citations.Invalid.Count > 0)
        {
            return DraftDecisionResult.InvalidCitations;
        }
        var comment = WithSources(citations.Text, draft.Sources.Where(s => citations.Cited.Contains(s.Number)));
        if (comment.Length > MaxCommentLength)
        {
            return DraftDecisionResult.TooLong;
        }

        // Dois SaveChanges (o do comentário, no TicketService, e o do rascunho) numa só transação.
        // Este projeto não tem notas internas (aula 11): o comentário é sempre público.
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        var (result, _) = await _tickets.AddCommentAsync(ticketId, userId, comment, ct);
        if (result != AddCommentResult.Added)
        {
            return result switch
            {
                AddCommentResult.TicketNotFound => DraftDecisionResult.NotFound,
                AddCommentResult.TicketResolved => DraftDecisionResult.TicketResolved,
                _ => DraftDecisionResult.Conflict
            };
        }
        Decide(draft, DraftStatus.Published, userId);
        // Exercício C: o texto publicado (sem a lista das fontes), para comparar com o da IA.
        draft.PublishedText = citations.Text;
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return DraftDecisionResult.Done;
    }

    public async Task<DraftDecisionResult> DiscardAsync(int ticketId, int draftId, string userId, CancellationToken ct = default)
    {
        var draft = await FindAsync(ticketId, draftId, ct);
        if (draft is null)
        {
            return DraftDecisionResult.NotFound;
        }
        if (draft.Status != DraftStatus.Pending)
        {
            return DraftDecisionResult.AlreadyDecided;
        }
        Decide(draft, DraftStatus.Discarded, userId);
        await _db.SaveChangesAsync(ct);
        return DraftDecisionResult.Done;
    }

    // Exercício C: quantos rascunhos foram usados, e quantos sem o técnico mexer no texto.
    public async Task<DraftStats> GetStatsAsync(CancellationToken ct = default)
    {
        var drafts = _db.AiDrafts.AsNoTracking();
        return new DraftStats(
            await drafts.CountAsync(ct),
            await drafts.CountAsync(d => d.Status == DraftStatus.Published, ct),
            // Sem alterações: o que se publicou é o que a IA escreveu (ou a pergunta que sugeriu).
            await drafts.CountAsync(d => d.Status == DraftStatus.Published
                && (d.PublishedText == d.Reply || (d.Reply == "" && d.PublishedText == d.ClarifyingQuestion)), ct),
            await drafts.CountAsync(d => d.Status == DraftStatus.Discarded, ct),
            await drafts.CountAsync(d => d.Status == DraftStatus.Pending, ct));
    }

    // O texto publicado leva no fim as fontes que cita, para o cliente saber de onde vem cada passo.
    public static string WithSources(string text, IEnumerable<TicketAiDraftSource> cited)
    {
        var lines = cited.OrderBy(s => s.Number).Select(s => $"[{s.Number}] {s.Heading}").ToList();
        return lines.Count == 0 ? text : $"{text}\n\nFontes:\n{string.Join("\n", lines)}";
    }

    private Task<TicketAiDraft?> FindAsync(int ticketId, int draftId, CancellationToken ct) =>
        _db.AiDrafts
            .Include(d => d.Sources)
            .FirstOrDefaultAsync(d => d.Id == draftId && d.TicketId == ticketId, ct);

    private static void Decide(TicketAiDraft draft, DraftStatus status, string userId)
    {
        draft.Status = status;
        draft.DecidedById = userId;
        draft.DecidedAt = DateTimeOffset.UtcNow;
    }
}
