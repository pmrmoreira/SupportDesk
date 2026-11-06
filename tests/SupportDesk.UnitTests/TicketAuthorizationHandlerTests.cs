using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using SupportDesk.Web.Authorization;
using SupportDesk.Web.Contracts;

namespace SupportDesk.UnitTests;

public class TicketAuthorizationHandlerTests
{
    private static async Task<bool> CanAsync(
        ClaimsPrincipal user, TicketDetails ticket, OperationAuthorizationRequirement operation)
    {
        var context = new AuthorizationHandlerContext([operation], user, ticket);
        await new TicketAuthorizationHandler().HandleAsync(context);
        return context.HasSucceeded;
    }

    [Fact]
    public async Task The_owner_can_read_and_comment_on_their_ticket()
    {
        var ana = TestUsers.Client("ana");
        var ticket = TestUsers.Ticket(createdById: "ana");

        Assert.True(await CanAsync(ana, ticket, TicketOperations.Read));
        Assert.True(await CanAsync(ana, ticket, TicketOperations.Comment));
    }

    [Fact]
    public async Task Another_client_cannot_read_the_ticket()
    {
        var rui = TestUsers.Client("rui");
        var ticket = TestUsers.Ticket(createdById: "ana");

        Assert.False(await CanAsync(rui, ticket, TicketOperations.Read));
    }

    [Fact]
    public async Task Only_technicians_change_the_status()
    {
        var ana = TestUsers.Client("ana");
        var technician = TestUsers.Technician("tec");
        var ticket = TestUsers.Ticket(createdById: "ana");

        Assert.False(await CanAsync(ana, ticket, TicketOperations.ChangeStatus));
        Assert.True(await CanAsync(technician, ticket, TicketOperations.ChangeStatus));
    }

    [Fact]
    public async Task A_ticket_without_owner_is_only_visible_to_technicians()
    {
        var ticket = TestUsers.Ticket(createdById: null);

        Assert.False(await CanAsync(TestUsers.Client("ana"), ticket, TicketOperations.Read));
        Assert.True(await CanAsync(TestUsers.Technician("tec"), ticket, TicketOperations.Read));
    }
}
