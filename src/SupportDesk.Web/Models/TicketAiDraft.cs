namespace SupportDesk.Web.Models;

// Um rascunho de resposta da IA para um pedido. Fica gravado com as fontes e com a decisão do técnico.
public class TicketAiDraft
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string Reply { get; set; } = string.Empty;
    public bool NeedsClarification { get; set; }
    public string? ClarifyingQuestion { get; set; }

    // Citações que apontavam para fontes que não existiam (retiradas do texto).
    public int InvalidCitations { get; set; }

    public string Model { get; set; } = string.Empty;
    public long? InputTokens { get; set; }
    public long? OutputTokens { get; set; }
    public int DurationMs { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public DraftStatus Status { get; set; } = DraftStatus.Pending;
    public string? DecidedById { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }

    // Exercício C: o texto que o técnico publicou, para comparar com o da IA.
    public string? PublishedText { get; set; }

    public List<TicketAiDraftSource> Sources { get; set; } = [];
}

// Uma fonte enviada ao modelo, guardada como estava: os trechos são recriados quando o artigo muda (aula 17).
public class TicketAiDraftSource
{
    public int Id { get; set; }
    public int DraftId { get; set; }
    public int Number { get; set; }
    public string ArticleSlug { get; set; } = string.Empty;
    public string Heading { get; set; } = string.Empty;
}

public enum DraftStatus
{
    Pending,
    Published,
    Discarded
}
