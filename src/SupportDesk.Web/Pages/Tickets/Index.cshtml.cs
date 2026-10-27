using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportDesk.Web.Contracts;
using SupportDesk.Web.Models;
using SupportDesk.Web.Services;

namespace SupportDesk.Web.Pages.Tickets;

public class IndexModel : PageModel
{
    private readonly ITicketService _tickets;

    public IndexModel(ITicketService tickets)
    {
        _tickets = tickets;
    }

    [BindProperty(SupportsGet = true)]
    public TicketStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search {  get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; }

    public PagedResult<TicketListItem> Result { get; private set; } = new([], 1, 10, 0);

    public async Task OnGetAsync(CancellationToken ct)
    {
        var query = new TicketQuery
        {
            Status = Status,
            Search = Search,
            Page = Math.Max(PageNumber, 1),
            PageSize = 10
        };
        Result = await _tickets.SearchAsync(query, ct);
    }
}
