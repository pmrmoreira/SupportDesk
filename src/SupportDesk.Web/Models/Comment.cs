namespace SupportDesk.Web.Models;

public class Comment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
