namespace SupportDesk.Web.Models;

// O último resumo da IA de cada pedido. As listas ficam em colunas text[] do PostgreSQL.
public class TicketAiSummary
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string Problem { get; set; } = string.Empty;
    public List<string> ActionsTaken { get; set; } = [];
    public List<string> MissingInformation { get; set; } = [];

    public string Model { get; set; } = string.Empty;
    public long? InputTokens { get; set; }
    public long? OutputTokens { get; set; }
    public int DurationMs { get; set; }
    public int IncludedComments { get; set; }
    public int OmittedComments { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // O UpdatedAt do pedido quando o resumo foi feito: se o pedido mudou depois, o resumo está desatualizado.
    public DateTimeOffset SourceUpdatedAt { get; set; }
}
