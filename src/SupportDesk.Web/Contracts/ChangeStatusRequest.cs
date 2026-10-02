using SupportDesk.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Web.Contracts;

public class ChangeStatusRequest
{
    [Required(ErrorMessage = "O estado é obrigatório")]
    public TicketStatus? Status { get; set; }
}
