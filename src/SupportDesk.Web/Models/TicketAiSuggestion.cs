namespace SupportDesk.Web.Models;

// Uma sugestão da IA para um pedido. Fica gravada, com o que custou, mesmo que ninguém a aceite.
public class TicketAiSuggestion
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public TicketCategory Category { get; set; }
    public int Priority { get; set; }
    public string Summary { get; set; } = string.Empty;

    // De onde veio e quanto custou.
    public string Model { get; set; } = string.Empty;
    public long? InputTokens { get; set; }
    public long? OutputTokens { get; set; }
    public int DurationMs { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // A decisão do técnico.
    public SuggestionDecision Decision { get; set; } = SuggestionDecision.Pending;
    public string? DecidedById { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
}

public enum SuggestionDecision
{
    Pending,
    Accepted,
    Rejected
}
