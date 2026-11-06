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
    string? CreatedByName
    );   // aula 10: null nos pedidos criados antes de haver contas

public record CommentResponse(int Id, string Author, string Text, DateTimeOffset CreatedAt);
