using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportDesk.Web.Authorization;
using SupportDesk.Web.Contracts;
using SupportDesk.Web.Models;
using SupportDesk.Web.Services;

namespace SupportDesk.Web.Pages.Tickets;

// Aula 9: editar o título, a descrição e a prioridade de um pedido.
public class EditModel : PageModel
{
    private readonly ITicketService _tickets;
    private readonly IAuthorizationService _authorization;

    public EditModel(ITicketService tickets, IAuthorizationService authorization)
    {
        _tickets = tickets;
        _authorization = authorization;
    }

    [BindProperty]
    public EditTicketRequest Input { get; set; } = new();

    public int Id { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(id, ct);
        if (ticket is null || !await CanAsync(ticket, TicketOperations.Read))
        {
            return NotFound();
        }
        if (!await CanAsync(ticket, TicketOperations.Edit))
        {
            return Forbid();
        }
        if (ticket.Status == TicketStatus.Resolved)
        {
            TempData["Error"] = "Não é possível editar um pedido resolvido. Reabre-o primeiro.";
            return RedirectToPage("./Details", new { id });
        }

        Id = id;
        // A versão vai num campo escondido: no POST, diz que versão do pedido a pessoa viu.
        Input = new EditTicketRequest
        {
            Title = ticket.Title,
            Description = ticket.Description,
            Priority = ticket.Priority,
            Version = ticket.Version
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(id, ct);
        if (ticket is null || !await CanAsync(ticket, TicketOperations.Read))
        {
            return NotFound();
        }
        if (!await CanAsync(ticket, TicketOperations.Edit))
        {
            return Forbid();
        }

        Id = id;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _tickets.UpdateAsync(
            id, Input.Title.Trim(), Input.Description?.Trim() ?? string.Empty, Input.Priority, Input.Version, ct);
        switch (result)
        {
            case UpdateTicketResult.Updated:
                TempData["Message"] = "Pedido atualizado.";
                return RedirectToPage("./Details", new { id });
            case UpdateTicketResult.NotFound:
                return NotFound();
            case UpdateTicketResult.TicketResolved:
                ModelState.AddModelError(string.Empty, "Não é possível editar um pedido resolvido. Reabre-o primeiro.");
                break;
            default:
                ModelState.AddModelError(string.Empty,
                    "Outra pessoa alterou este pedido depois de o abrires. Abre-o outra vez para veres a versão atual.");
                break;
        }
        return Page();
    }

    private async Task<bool> CanAsync(TicketDetails ticket, OperationAuthorizationRequirement operation) =>
        (await _authorization.AuthorizeAsync(User, ticket, operation)).Succeeded;
}
