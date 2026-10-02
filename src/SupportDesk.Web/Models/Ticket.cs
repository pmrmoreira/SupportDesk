using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportDesk.Web.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; }
    }
}
