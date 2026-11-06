namespace SupportDesk.Web.Models;

// As regras de mudança de estado num só sítio: o serviço e as páginas usam as mesmas.
public static class TicketWorkflow
{
    public static bool CanChange(TicketStatus from, TicketStatus to) => (from, to) switch
    {
        (TicketStatus.Open, TicketStatus.InProgress) => true,
        (TicketStatus.InProgress, TicketStatus.Resolved) => true,
        (TicketStatus.InProgress, TicketStatus.Open) => true,
        (TicketStatus.Resolved, TicketStatus.Open) => true,
        
        _ => false
    };

    public static TicketStatus[] NextStatuses(TicketStatus from) =>
        Enum.GetValues<TicketStatus>().Where(to => CanChange(from, to)).ToArray();
}
