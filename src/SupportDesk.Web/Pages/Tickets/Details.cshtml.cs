using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportDesk.Web.Contracts;
using SupportDesk.Web.Services;

namespace SupportDesk.Web.Pages.Tickets;

public class DetailsModel : PageModel
{
    private readonly ITicketService _tickets;

    public DetailsModel(ITicketService tickets)
    {
        _tickets = tickets;
    }

    public TicketDetails Ticket { get; private set; } = default!;
    public CommentResponse[] Comments { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(id, ct);
        if (ticket is null)
        {
            return NotFound();
        }

        Ticket = ticket;
        Comments = await _tickets.GetCommentsAsync(id, ct);
        return Page();
    }
}
