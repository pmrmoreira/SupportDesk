using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportDesk.Web.Infrastructure;
using SupportDesk.Web.Models;
using SupportDesk.Web.Services;

namespace SupportDesk.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ITicketService _tickets;

    public IndexModel(ITicketService tickets)
    {
        _tickets = tickets;
    }

    // Vazio para quem não tem sessão: a página mostra o convite em vez dos números.
    public Dictionary<TicketStatus, int> Counts { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        // Aula 10, ciclo 3: o início está fora de /Tickets e abre sem sessão.
        // Sem este if, OwnerFilter() pedia o id a um visitante sem claims.
        if (User.Identity?.IsAuthenticated == true)
        {
            Counts = await _tickets.CountByStatusAsync(User.OwnerFilter(), ct);
        }
    }
}
