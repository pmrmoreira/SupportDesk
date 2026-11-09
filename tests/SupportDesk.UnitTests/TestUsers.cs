using System.Security.Claims;
using SupportDesk.Web.Contracts;
using SupportDesk.Web.Models;

namespace SupportDesk.UnitTests;

public static class TestUsers
{
    public static ClaimsPrincipal Client(string id) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id)], authenticationType: "Test"));

    public static ClaimsPrincipal Technician(string id) =>
        new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, id), new Claim(ClaimTypes.Role, Roles.Technician)],
            authenticationType: "Test"));

    // Um pedido com valores neutros; só interessa quem o criou e o estado.
    public static TicketDetails Ticket(string? createdById, TicketStatus status = TicketStatus.Open) =>
        new(Id: 1, Title: "Pedido de teste", Description: "", Status: status, Priority: 2,
            CreatedAt: DateTimeOffset.UtcNow, UpdatedAt: DateTimeOffset.UtcNow, Version: 1,
            CreatedById: createdById, CreatedByName: null, Category: null);
}
