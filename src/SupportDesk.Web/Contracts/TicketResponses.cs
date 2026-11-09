using SupportDesk.Web.Models;

namespace SupportDesk.Web.Contracts;


public record TicketListItem(
    int Id,
    string Title,
    TicketStatus Status,
    int Priority,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,   
    int CommentCount);

public record TicketDetails(
    int Id,
    string Title,
    string Description,
    TicketStatus Status,
    int Priority,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    uint Version,
    string? CreatedById,
    string? CreatedByName,      // aula 10: null nos pedidos criados antes de haver contas
    TicketCategory? Category);  // aula 15: null até um técnico aceitar uma sugestão da IA
public record SuggestionResponse(
    int Id,
    TicketCategory Category,
    int Priority,
    string Summary,
    string Model,
    long? InputTokens,
    long? OutputTokens,
    int DurationMs,
    DateTimeOffset CreatedAt,
    SuggestionDecision Decision,
    string? DecidedByName,
    DateTimeOffset? DecidedAt);

public record CommentResponse(int Id, string Author, string Text, DateTimeOffset CreatedAt);

// Aula 16: o último resumo da IA de um pedido, como as páginas o leem.
public record SummaryResponse(
    string Problem,
    IReadOnlyList<string> ActionsTaken,
    IReadOnlyList<string> MissingInformation,
    string Model,
    long? InputTokens,
    long? OutputTokens,
    int DurationMs,
    int IncludedComments,
    int OmittedComments,
    DateTimeOffset CreatedAt,
    DateTimeOffset SourceUpdatedAt);
