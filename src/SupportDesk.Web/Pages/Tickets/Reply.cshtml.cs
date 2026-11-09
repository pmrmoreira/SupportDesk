using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportDesk.Web.Ai;
using SupportDesk.Web.Authorization;
using SupportDesk.Web.Contracts;
using SupportDesk.Web.Infrastructure;
using SupportDesk.Web.Services;

namespace SupportDesk.Web.Pages.Tickets;

// Rascunho de resposta com a IA (só técnicos): gerar, rever, publicar ou descartar.
// Página própria: o detalhe do pedido já tem seis dependências.
public class ReplyModel : PageModel
{
    private readonly ITicketService _tickets;
    private readonly IAuthorizationService _authorization;
    private readonly ReplyDrafter _drafter;
    private readonly IDraftService _drafts;

    public ReplyModel(ITicketService tickets, IAuthorizationService authorization,
        ReplyDrafter drafter, IDraftService drafts)
    {
        _tickets = tickets;
        _authorization = authorization;
        _drafter = drafter;
        _drafts = drafts;
    }

    public TicketDetails Ticket { get; private set; } = default!;
    public DraftResponse? Draft { get; private set; }

    // O texto da caixa: o rascunho da IA, depois de o técnico o rever.
    [BindProperty]
    public string? Text { get; set; }

    public string? PublishError { get; private set; }

    // Exercício A: com falta de informação e sem resposta, o rascunho é uma pergunta ao cliente.
    public bool AsksQuestion(DraftResponse draft) =>
        draft.NeedsClarification && string.IsNullOrWhiteSpace(draft.Reply);

    // A caixa começa com a pergunta, nesse caso; senão, com a resposta.
    public string BoxText(DraftResponse draft) =>
        Text ?? (AsksQuestion(draft) ? draft.ClarifyingQuestion ?? string.Empty : draft.Reply);

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        return await LoadAsync(id, ct) ?? Page();
    }

    public async Task<IActionResult> OnPostGenerateAsync(int id, CancellationToken ct)
    {
        if (await LoadAsync(id, ct) is { } stop)
        {
            return stop;
        }

        var result = await _drafter.DraftAsync(Ticket.Title, Ticket.Description, ct);
        if (result.Draft is null)
        {
            TempData["Error"] = $"A IA não fez o rascunho: {result.Error}";
        }
        else
        {
            await _drafts.SaveAsync(id, result, ct);
            TempData["Message"] = "Rascunho pronto. Revê o texto e as fontes antes de publicar.";
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostPublishAsync(int id, int draftId, CancellationToken ct)
    {
        if (await LoadAsync(id, ct) is { } stop)
        {
            return stop;
        }

        var result = await _drafts.PublishAsync(id, draftId, Text ?? string.Empty, User.GetUserId(), ct);
        switch (result)
        {
            case DraftDecisionResult.Done:
                TempData["Message"] = AsksQuestion(Draft!) ? "Pergunta enviada ao cliente." : "Resposta publicada no pedido.";
                return RedirectToPage("./Details", new { id });
            case DraftDecisionResult.NotFound:
                return NotFound();
            case DraftDecisionResult.AlreadyDecided:
                TempData["Error"] = "Este rascunho já tinha sido decidido.";
                return RedirectToPage(new { id });
        }

        // Os outros casos voltam à página com o texto editado na caixa, para não se perder.
        PublishError = result switch
        {
            DraftDecisionResult.Empty => "Escreve a resposta antes de publicar.",
            DraftDecisionResult.InvalidCitations =>
                "O texto cita uma fonte que não existe. Confirma os números entre parênteses retos.",
            DraftDecisionResult.TooLong => "A resposta, com a lista das fontes, passa dos 2000 caracteres de um comentário.",
            DraftDecisionResult.TicketResolved => "Não é possível comentar um pedido resolvido. Reabre-o primeiro.",
            _ => "Outra pessoa alterou este pedido ao mesmo tempo. Tenta outra vez."
        };
        return Page();
    }

    public async Task<IActionResult> OnPostDiscardAsync(int id, int draftId, CancellationToken ct)
    {
        if (await LoadAsync(id, ct) is { } stop)
        {
            return stop;
        }

        var result = await _drafts.DiscardAsync(id, draftId, User.GetUserId(), ct);
        if (result == DraftDecisionResult.NotFound)
        {
            return NotFound();
        }
        TempData[result == DraftDecisionResult.Done ? "Message" : "Error"] =
            result == DraftDecisionResult.Done ? "Rascunho descartado." : "Este rascunho já tinha sido decidido.";
        return RedirectToPage(new { id });
    }

    // Carrega o pedido e confirma as permissões. null = pode continuar; senão, a resposta a devolver.
    private async Task<IActionResult?> LoadAsync(int id, CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(id, ct);
        if (ticket is null || !(await _authorization.AuthorizeAsync(User, ticket, TicketOperations.Read)).Succeeded)
        {
            return NotFound();
        }
        Ticket = ticket;
        if (!(await _authorization.AuthorizeAsync(User, ticket, TicketOperations.DraftReply)).Succeeded)
        {
            return Forbid();
        }
        Draft = await _drafts.GetLatestAsync(id, ct);
        return null;
    }
}
