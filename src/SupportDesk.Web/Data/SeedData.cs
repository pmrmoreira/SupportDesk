using Microsoft.EntityFrameworkCore;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Data;

public class SeedData
{
    private static Ticket[] InitialTickets() =>
    [
        new Ticket
        {
            Title = "Não consigo entrar",
            Description = "A aplicação diz que a palavra-passe está errada.",
            Status = TicketStatus.Open,
            Priority = 1,
            CreatedAt = new DateTimeOffset(2026, 9, 28, 8, 15, 0, TimeSpan.Zero)
        },
        new Ticket
        {
            Title = "Fatura com valor incorreto",
            Description = "O valor de setembro vem em duplicado.",
            Status = TicketStatus.InProgress,
            Priority = 2,
            CreatedAt = new DateTimeOffset(2026, 9, 28, 9, 40, 0, TimeSpan.Zero)
        },
        new Ticket
        {
            Title = "Impressora indisponível",
            Status = TicketStatus.Open,
            Priority = 3,
            CreatedAt = new DateTimeOffset(2026, 9, 28, 13, 5, 0, TimeSpan.Zero)
        }
    ];

    public static void Seed(DbContext context)
    {
        var tickets = context.Set<Ticket>();
        if (!tickets.Any())
        {
            tickets.AddRange(InitialTickets());
            context.SaveChanges();
        }
    }
    public static async Task SeedAsync(DbContext context, CancellationToken ct)
    {
        var tickets = context.Set<Ticket>();
        if (!await tickets.AnyAsync(ct))
        {
            tickets.AddRange(InitialTickets());
            await context.SaveChangesAsync(ct);
        }
    }
}
