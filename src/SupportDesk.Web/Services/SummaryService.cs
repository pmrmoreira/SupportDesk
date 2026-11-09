using Microsoft.EntityFrameworkCore;
using SupportDesk.Web.Ai;
using SupportDesk.Web.Contracts;
using SupportDesk.Web.Data;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Services;

public class SummaryService : ISummaryService
{
    private readonly SupportDeskDbContext _db;

    public SummaryService(SupportDeskDbContext db)
    {
        _db = db;
    }

    // Projeção direta para o DTO, sem tracking (aula 7). As listas vêm das colunas text[].
    public Task<SummaryResponse?> GetAsync(int ticketId, CancellationToken ct = default) =>
        _db.AiSummaries
            .AsNoTracking()
            .Where(s => s.TicketId == ticketId)
            .Select(s => new SummaryResponse(
                s.Problem, s.ActionsTaken, s.MissingInformation, s.Model, s.InputTokens, s.OutputTokens,
                s.DurationMs, s.IncludedComments, s.OmittedComments, s.CreatedAt, s.SourceUpdatedAt))
            .FirstOrDefaultAsync(ct);

    // Substitui em vez de acumular: procura o resumo do pedido, cria-o se não houver, e reescreve tudo.
    // O índice único em TicketId é a rede por baixo: a base nunca aceita dois resumos do mesmo pedido.
    public async Task SaveAsync(
        int ticketId, TicketSummary summary, SummaryResult call, DateTimeOffset sourceUpdatedAt, CancellationToken ct = default)
    {
        var entity = await _db.AiSummaries.FirstOrDefaultAsync(s => s.TicketId == ticketId, ct);
        if (entity is null)
        {
            entity = new TicketAiSummary { TicketId = ticketId };
            _db.AiSummaries.Add(entity);
        }
        entity.Problem = summary.Problem;
        entity.ActionsTaken = summary.ActionsTaken;
        entity.MissingInformation = summary.MissingInformation;
        entity.Model = call.Model ?? "?";
        entity.InputTokens = call.InputTokens;
        entity.OutputTokens = call.OutputTokens;
        entity.DurationMs = (int)call.Elapsed.TotalMilliseconds;
        entity.IncludedComments = call.IncludedComments;
        entity.OmittedComments = call.OmittedComments;
        entity.CreatedAt = DateTimeOffset.UtcNow;
        entity.SourceUpdatedAt = sourceUpdatedAt;
        await _db.SaveChangesAsync(ct);
    }
}
