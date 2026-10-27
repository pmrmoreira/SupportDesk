using SupportDesk.Web.Models;

namespace SupportDesk.Web.Infrastructure;

public static class DisplayFormat
{
    private static readonly TimeZoneInfo Lisbon = TimeZoneInfo.FindSystemTimeZoneById("Europe/Lisbon");

    public static string Status(TicketStatus status) => status switch
    {
        TicketStatus.Open => "Aberto",
        TicketStatus.InProgress => "Em curso",
        TicketStatus.Resolved => "Resolvido",
        _ => status.ToString()
    };

    public static string Priority(int priority) => priority switch
    {
        1 => "Alta",
        2 => "Normal",
        3 => "Baixa",
        _ => priority.ToString()
    };

    public static string DateTime(DateTimeOffset value) =>
        TimeZoneInfo.ConvertTime(value, Lisbon).ToString("dd/MM/yyyy HH:mm");
}
