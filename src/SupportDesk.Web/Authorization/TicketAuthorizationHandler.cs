using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using SupportDesk.Web.Contracts;
using SupportDesk.Web.Infrastructure;

namespace SupportDesk.Web.Authorization;

public class TicketAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, TicketDetails>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, TicketDetails ticket)
    {
        var isTechnician = context.User.IsTechnician();
        var isOwner = ticket.CreatedById is not null && ticket.CreatedById == context.User.GetUserId();

        var allowed = requirement.Name switch
        {
            nameof(TicketOperations.Read) => isTechnician || isOwner,
            nameof(TicketOperations.Comment) => isTechnician || isOwner,
            nameof(TicketOperations.Edit) => isTechnician || isOwner,
            nameof(TicketOperations.ChangeStatus) => isTechnician,
            nameof(TicketOperations.Classify) => isTechnician,
            nameof(TicketOperations.Summarize) => isTechnician,
            _ => false
        };

        if (allowed)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}