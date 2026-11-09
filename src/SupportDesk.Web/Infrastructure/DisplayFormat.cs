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
    // O texto do botão que muda o pedido de um estado para outro.
    public static string Transition(TicketStatus from, TicketStatus to) => (from, to) switch
    {
        (_, TicketStatus.InProgress) => "Começar a tratar",
        (_, TicketStatus.Resolved) => "Marcar como resolvido",
        (TicketStatus.Resolved, TicketStatus.Open) => "Reabrir",
        _ => "Devolver à fila"
    };
    public static string Category(TicketCategory category) => category switch
    {
        TicketCategory.Access => "Acesso",
        TicketCategory.Billing => "Faturação",
        TicketCategory.Hardware => "Equipamento",
        TicketCategory.Software => "Software",
        TicketCategory.Network => "Rede",
        TicketCategory.Other => "Outro",
        _ => category.ToString()
    };


    public static string DateTime(DateTimeOffset value) =>
        TimeZoneInfo.ConvertTime(value, Lisbon).ToString("dd/MM/yyyy HH:mm");
}
