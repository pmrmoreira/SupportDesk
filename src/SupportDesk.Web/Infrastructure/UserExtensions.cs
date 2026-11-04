using System.Security.Claims;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Infrastructure;

public static class UserExtensions
{
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public static bool IsTechnician(this ClaimsPrincipal user) =>
        user.IsInRole(Roles.Technician);

    // Filtro das listas: os técnicos veem todos os pedidos (null), os clientes só os seus.
    public static string? OwnerFilter(this ClaimsPrincipal user) =>
        user.IsTechnician() ? null : user.GetUserId();
}
