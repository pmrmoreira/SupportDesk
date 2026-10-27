using SupportDesk.Web.Models;

namespace SupportDesk.Web.Contracts;

// Contratos de saída da API (aula 7, ciclo 1): o JSON deixa de ser a entidade da base.
public record TicketListItem(
    int Id,
    string Title,
    TicketStatus Status,
    int Priority,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,   // era "UpdateAt": o JSON saía com outro nome
    int CommentCount);

public record TicketDetails(
    int Id,
    string Title,
    string Description,
    TicketStatus Status,
    int Priority,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record CommentResponse(int Id, string Author, string Text, DateTimeOffset CreatedAt);
