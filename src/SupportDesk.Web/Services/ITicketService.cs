using SupportDesk.Web.Models;
using static SupportDesk.Web.Services.TicketService;

namespace SupportDesk.Web.Services;

public interface ITicketService
{
    Ticket[] GetAll(TicketStatus? status = null);
    Ticket? GetById(int id);
    int Count();
    Ticket Create(string title, string description);
    StatusChangeResult ChangeStatus(int id, TicketStatus newStatus);
    (AddCommentResult Result, Comment? Comment) AddComment(int ticketId, string author, string text);
    Comment[] GetComments(int ticketId);
    
}
