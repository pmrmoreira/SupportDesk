using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace SupportDesk.Web.Authorization;

public static class TicketOperations
{
    public static readonly OperationAuthorizationRequirement Read = new() { Name = nameof(Read) };
    public static readonly OperationAuthorizationRequirement Comment = new() { Name = nameof(Comment) };
    public static readonly OperationAuthorizationRequirement Edit = new() { Name = nameof(Edit) };
    public static readonly OperationAuthorizationRequirement ChangeStatus = new() { Name = nameof(ChangeStatus) };
    // Aula 15: pedir e decidir sugestões da IA.
    public static readonly OperationAuthorizationRequirement Classify = new() { Name = nameof(Classify) };
}
