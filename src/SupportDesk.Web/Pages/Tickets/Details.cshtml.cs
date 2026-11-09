using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportDesk.Web.Ai;
using SupportDesk.Web.Authorization;
using SupportDesk.Web.Contracts;
using SupportDesk.Web.Infrastructure;
using SupportDesk.Web.Models;
using SupportDesk.Web.Services;

namespace SupportDesk.Web.Pages.Tickets;

public class DetailsModel : PageModel
{
    private readonly ITicketService _tickets;
    private readonly IAuthorizationService _authorization;
    private readonly TicketClassifier _classifier;
    private readonly ISuggestionService _suggestions;
    private readonly TicketSummarizer _summarizer;
    private readonly ISummaryService _summaries;

    public DetailsModel(ITicketService tickets, IAuthorizationService authorization,
        TicketClassifier classifier, ISuggestionService suggestions,
        TicketSummarizer summarizer, ISummaryService summaries)
    {
        _tickets = tickets;
        _authorization = authorization;
        _classifier = classifier;
        _suggestions = suggestions;
        _summarizer = summarizer;
        _summaries = summaries;
    }

    public TicketDetails Ticket { get; private set; } = default!;
    public CommentResponse[] Comments { get; private set; } = [];
    public AddCommentRequest NewComment { get; set; } = new();
    public bool CanChangeStatus { get; private set; }
    public bool CanEdit { get; private set; }
    // Aula 15, ciclo 2: só técnicos pedem sugestões à IA e veem o cartão.
    public bool CanClassify { get; private set; }
    public SuggestionResponse? Suggestion { get; private set; }
    // Aula 16: o resumo da conversa, só para técnicos.
    public bool CanSummarize { get; private set; }
    public SummaryResponse? Summary { get; private set; }

    // Aula 16, ciclo 3: o pedido mudou (comentário, estado, edição) depois de o resumo ser feito.
    public bool IsSummaryStale => Summary is not null && Summary.SourceUpdatedAt < Ticket.UpdatedAt;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        return await LoadAsync(id, ct) ? Page() : NotFound();
    }

    // Formulário do comentário: asp-page-handler="Comment"
    public async Task<IActionResult> OnPostCommentAsync(int id, AddCommentRequest newComment, CancellationToken ct)
    {
        if (!await LoadAsync(id, ct))
        {
            return NotFound();
        }
        if (!await CanAsync(TicketOperations.Comment))
        {
            return Forbid();
        }

        NewComment = newComment;
        if (ModelState.IsValid)
        {
            var (result, _) = await _tickets.AddCommentAsync(id, User.GetUserId(), newComment.Text.Trim(), ct);
            switch (result)
            {
                case AddCommentResult.Added:
                    TempData["Message"] = "Comentário adicionado.";
                    return RedirectToPage(new { id });
                case AddCommentResult.TicketNotFound:
                    return NotFound();
                case AddCommentResult.TicketResolved:
                    ModelState.AddModelError(string.Empty, "Não é possível comentar um pedido resolvido. Reabre-o primeiro.");
                    break;
                default:
                    ModelState.AddModelError(string.Empty, "Outra pessoa alterou este pedido ao mesmo tempo. Tenta outra vez.");
                    break;
            }
        }
        return Page();
    }

    // Botões de estado: asp-page-handler="Status" (só técnicos)
    public async Task<IActionResult> OnPostStatusAsync(int id, TicketStatus status, CancellationToken ct)
    {
        if (!await LoadAsync(id, ct))
        {
            return NotFound();
        }
        if (!CanChangeStatus)
        {
            return Forbid();
        }

        var result = await _tickets.ChangeStatusAsync(id, status, ct);
        switch (result)
        {
            case StatusChangeResult.NotFound:
                return NotFound();
            case StatusChangeResult.Changed:
                TempData["Message"] = $"Estado alterado para {DisplayFormat.Status(status)}.";
                break;
            case StatusChangeResult.NotAllowed:
                TempData["Error"] = "Essa mudança de estado não é permitida.";
                break;
            default:
                TempData["Error"] = "Outra pessoa alterou este pedido ao mesmo tempo. Tenta outra vez.";
                break;
        }
        return RedirectToPage(new { id });
    }

    // Botão "Pedir sugestão à IA" (só técnicos): classifica e grava a sugestão, sem mudar o pedido.
    public async Task<IActionResult> OnPostSuggestAsync(int id, CancellationToken ct)
    {
        if (!await LoadAsync(id, ct))
        {
            return NotFound();
        }
        if (!CanClassify)
        {
            return Forbid();
        }

        var result = await _classifier.ClassifyAsync(Ticket.Title, Ticket.Description, ct);
        if (result.Classification is null)
        {
            TempData["Error"] = $"A IA não deu uma sugestão: {result.Error}";
        }
        else
        {
            await _suggestions.AddAsync(id, result.Classification, result, ct);
            TempData["Message"] = "A IA sugeriu uma classificação. Revê-a antes de aceitar.";
        }
        return RedirectToPage(new { id });
    }

    // Aula 15, ciclo 3 — botões "Aceitar" e "Rejeitar" (só técnicos): a decisão é de uma pessoa.
    // Os dois formulários usam este handler; o campo escondido "accept" diz qual foi o botão.
    public async Task<IActionResult> OnPostDecideAsync(int id, int suggestionId, bool accept, CancellationToken ct)
    {
        if (!await LoadAsync(id, ct))
        {
            return NotFound();
        }
        if (!CanClassify)
        {
            return Forbid();
        }

        var result = await _suggestions.DecideAsync(id, suggestionId, accept, User.GetUserId(), ct);
        switch (result)
        {
            case DecideResult.Applied:
                TempData["Message"] = accept ? "Sugestão aceite: o pedido foi classificado." : "Sugestão rejeitada.";
                break;
            case DecideResult.NotFound:
                return NotFound();
            case DecideResult.AlreadyDecided:
                TempData["Error"] = "Essa sugestão já tinha sido decidida.";
                break;
            default:
                TempData["Error"] = "Outra pessoa alterou este pedido ao mesmo tempo. Tenta outra vez.";
                break;
        }
        return RedirectToPage(new { id });
    }

    // Aula 16 — botão "Resumir conversa" (só técnicos): o resumo substitui o anterior.
    public async Task<IActionResult> OnPostSummarizeAsync(int id, CancellationToken ct)
    {
        if (!await LoadAsync(id, ct))
        {
            return NotFound();
        }
        if (!CanSummarize)
        {
            return Forbid();
        }

        var result = await _summarizer.SummarizeAsync(Ticket, Comments, ct);
        if (result.Summary is null)
        {
            TempData["Error"] = $"A IA não fez o resumo: {result.Error}";
        }
        else
        {
            // O UpdatedAt lido pelo LoadAsync ANTES de chamar o modelo: um comentário que chegue durante
            // a chamada deixa o resumo marcado como desatualizado, como deve ser (o modelo não o leu).
            await _summaries.SaveAsync(id, result.Summary, result, Ticket.UpdatedAt, ct);
            TempData["Message"] = "Resumo da conversa atualizado.";
        }
        return RedirectToPage(new { id });
    }

    // Aula 16, exercício A: depois de ler o resumo, o técnico guarda-o no pedido.
    // No curso fica como nota interna; este projeto ainda não tem notas internas (aula 11),
    // por isso fica como comentário normal, que o cliente também vê.
    public async Task<IActionResult> OnPostSaveSummaryAsync(int id, CancellationToken ct)
    {
        if (!await LoadAsync(id, ct))
        {
            return NotFound();
        }
        if (!CanSummarize)
        {
            return Forbid();
        }
        if (Summary is null)
        {
            TempData["Error"] = "Ainda não há resumo para guardar.";
            return RedirectToPage(new { id });
        }

        // Guarda-se o resumo que o LoadAsync leu da base, o mesmo do ecrã: sem chamar o modelo outra vez.
        var (result, _) = await _tickets.AddCommentAsync(id, User.GetUserId(), SummaryNote.Format(Summary), ct);
        switch (result)
        {
            case AddCommentResult.Added:
                TempData["Message"] = "Resumo guardado no pedido como comentário.";
                break;
            case AddCommentResult.TicketNotFound:
                return NotFound();
            case AddCommentResult.TicketResolved:
                TempData["Error"] = "Não é possível comentar um pedido resolvido. Reabre-o primeiro.";
                break;
            default:
                TempData["Error"] = "Outra pessoa alterou este pedido ao mesmo tempo. Tenta outra vez.";
                break;
        }
        return RedirectToPage(new { id });
    }

    // Carrega o pedido só se o utilizador o puder ver: um pedido de outro cliente dá 404, como um que não existe.
    private async Task<bool> LoadAsync(int id, CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(id, ct);
        if (ticket is null)
        {
            return false;
        }
        Ticket = ticket;
        if (!await CanAsync(TicketOperations.Read))
        {
            return false;
        }

        CanChangeStatus = await CanAsync(TicketOperations.ChangeStatus);
        CanEdit = await CanAsync(TicketOperations.Edit);
        CanClassify = await CanAsync(TicketOperations.Classify);
        if (CanClassify)
        {
            Suggestion = await _suggestions.GetLatestAsync(id, ct);
        }
        // Aula 16: CanSummarize decide o cartão, o botão e a leitura da base. Para um cliente, nem há consulta.
        CanSummarize = await CanAsync(TicketOperations.Summarize);
        if (CanSummarize)
        {
            Summary = await _summaries.GetAsync(id, ct);
        }
        Comments = await _tickets.GetCommentsAsync(id, ct);
        return true;
    }

    private async Task<bool> CanAsync(OperationAuthorizationRequirement operation) =>
        (await _authorization.AuthorizeAsync(User, Ticket, operation)).Succeeded;
}
