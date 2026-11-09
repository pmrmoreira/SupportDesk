using Microsoft.EntityFrameworkCore;
using SupportDesk.Web.Ai;
using SupportDesk.Web.Contracts;
using SupportDesk.Web.Data;
using SupportDesk.Web.Infrastructure;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Services;

public class SuggestionService : ISuggestionService
{
    private readonly SupportDeskDbContext _db;

    public SuggestionService(SupportDeskDbContext db)
    {
        _db = db;
    }

    // Grava uma sugestão já validada, com o modelo, os tokens e o tempo da chamada. Não toca no pedido.
    public async Task<SuggestionResponse> AddAsync(
        int ticketId, TicketClassification classification, ClassificationResult call, CancellationToken ct = default)
    {
        var suggestion = new TicketAiSuggestion
        {
            TicketId = ticketId,
            Category = classification.Category,
            Priority = classification.Priority,
            Summary = classification.Summary,
            Model = call.Model ?? "?",
            InputTokens = call.InputTokens,
            OutputTokens = call.OutputTokens,
            DurationMs = (int)call.Elapsed.TotalMilliseconds,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.AiSuggestions.Add(suggestion);
        await _db.SaveChangesAsync(ct);
        return (await GetAsync(suggestion.Id, ct))!;
    }

    // A mais recente do pedido (o maior Id), ou null se ainda não houver nenhuma.
    // O (int?) faz o FirstOrDefaultAsync devolver null em vez de 0 quando não há linhas.
    public async Task<SuggestionResponse?> GetLatestAsync(int ticketId, CancellationToken ct = default)
    {
        var latestId = await _db.AiSuggestions
            .Where(s => s.TicketId == ticketId)
            .OrderByDescending(s => s.Id)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(ct);
        return latestId is null ? null : await GetAsync(latestId.Value, ct);
    }

    // A decisão é de uma pessoa: aceitar aplica a sugestão ao pedido; rejeitar só regista.
    public async Task<DecideResult> DecideAsync(
        int ticketId, int suggestionId, bool accept, string userId, CancellationToken ct = default)
    {
        // Procura-se pelo par (sugestão, pedido): o id vem de um campo escondido, e o browser pode mandar outro.
        var suggestion = await _db.AiSuggestions
            .FirstOrDefaultAsync(s => s.Id == suggestionId && s.TicketId == ticketId, ct);
        if (suggestion is null)
        {
            return DecideResult.NotFound;
        }
        // Uma sugestão decide-se uma vez: um segundo clique, ou um POST repetido, não aplica nada outra vez.
        if (suggestion.Decision != SuggestionDecision.Pending)
        {
            return DecideResult.AlreadyDecided;
        }

        var now = DateTimeOffset.UtcNow;
        suggestion.Decision = accept ? SuggestionDecision.Accepted : SuggestionDecision.Rejected;
        suggestion.DecidedById = userId;
        suggestion.DecidedAt = now;

        if (accept)
        {
            // Aceitar muda o pedido; fica um comentário do Sistema a dizer o quê e quem decidiu.
            var ticket = await _db.Tickets.FirstAsync(t => t.Id == ticketId, ct);
            ticket.Category = suggestion.Category;
            ticket.Priority = suggestion.Priority;
            ticket.UpdatedAt = now;
            var name = await DisplayNameAsync(userId, ct) ?? "?";
            _db.Comments.Add(new Comment
            {
                TicketId = ticketId,
                Author = "Sistema",
                Text = $"Classificação sugerida pela IA e aceite por {name}: {DisplayFormat.Category(suggestion.Category)}, " +
                       $"prioridade {DisplayFormat.Priority(suggestion.Priority)}.",
                CreatedAt = now
            });
        }

        // Tudo num só SaveChanges: a decisão, o pedido e o comentário ficam juntos, ou não fica nada.
        // Se outra pessoa gravou o pedido entretanto (o xmin da aula 7), o EF lança DbUpdateConcurrencyException.
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return DecideResult.Conflict;
        }
        return DecideResult.Applied;
    }

    // Projeção (aula 7): o nome de quem decidiu vem numa subconsulta, o DisplayName ou o email.
    private Task<SuggestionResponse?> GetAsync(int id, CancellationToken ct) =>
        _db.AiSuggestions
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SuggestionResponse(
                s.Id, s.Category, s.Priority, s.Summary, s.Model, s.InputTokens, s.OutputTokens, s.DurationMs,
                s.CreatedAt, s.Decision,
                _db.Users.Where(u => u.Id == s.DecidedById)
                    .Select(u => u.DisplayName == "" ? u.Email : u.DisplayName)
                    .FirstOrDefault(),
                s.DecidedAt))
            .FirstOrDefaultAsync(ct);

    // O nome de quem decide, para o comentário do Sistema: o DisplayName ou o email.
    private async Task<string?> DisplayNameAsync(string userId, CancellationToken ct) =>
        await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.DisplayName == "" ? u.Email : u.DisplayName)
            .FirstOrDefaultAsync(ct);
}
