using SupportDesk.Web.Models;
using static SupportDesk.Web.Services.TicketService;

namespace SupportDesk.Web.Services;

public interface ITicketService
{
    Ticket[] GetAll();
    Ticket? GetById(int id);
    int Count();
    Ticket Create(string title, string description);
    StatusChangeResult ChangeStatus(int id, TicketStatus newStatus);
    Comment? AddComment(int ticketId, string author, string text);
    Comment[] GetComments(int ticketId);
    
}
