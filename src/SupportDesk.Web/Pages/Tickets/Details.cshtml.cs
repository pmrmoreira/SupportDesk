using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

    public DetailsModel(ITicketService tickets, IAuthorizationService authorization)
    {
        _tickets = tickets;
        _authorization = authorization;
    }

    public TicketDetails Ticket { get; private set; } = default!;
    public CommentResponse[] Comments { get; private set; } = [];
    public AddCommentRequest NewComment { get; set; } = new();
    public bool CanChangeStatus { get; private set; }
    public bool CanEdit { get; private set; }

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
        Comments = await _tickets.GetCommentsAsync(id, ct);
        return true;
    }

    private async Task<bool> CanAsync(OperationAuthorizationRequirement operation) =>
        (await _authorization.AuthorizeAsync(User, Ticket, operation)).Succeeded;
}
