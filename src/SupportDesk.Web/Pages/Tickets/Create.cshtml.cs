using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportDesk.Web.Contracts;
using SupportDesk.Web.Services;
using System.Security.Claims;

namespace SupportDesk.Web.Pages.Tickets;

public class CreateModel : PageModel
{
    private readonly ITicketService _tickets;

    public CreateModel(ITicketService tickets)
    {
        _tickets = tickets;
    }

    [BindProperty]
    public CreateTicketRequest Input { get; set; } = new();

    public void OnGet()
    {

    }
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        // O id de quem tem sessão vem das claims do cookie. O ! diz o que o AuthorizeFolder garante: há sessão.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var ticket = await _tickets.CreateAsync(Input.Title.Trim(), Input.Description?.Trim() ?? string.Empty, Input.Priority, userId, ct);

        TempData["Message"] = "Pedido criado.";
        return RedirectToPage("./Details", new { id = ticket.Id});
    }
}
