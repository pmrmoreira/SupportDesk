using Microsoft.AspNetCore.Identity;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. O papel tem de existir antes de lá pores alguém.
        if (!await roleManager.RoleExistsAsync(Roles.Technician))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Technician));
        }

        // 2. As credenciais vêm da configuração (user-secrets), nunca do código.
        var email = configuration["Seed:TechnicianEmail"];
        var password = configuration["Seed:TechnicianPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Seed:TechnicianEmail/Seed:TechnicianPassword em falta; técnico de teste não criado.");
            return;
        }

        // 3. Só cria o técnico se ainda não existir.
        var technician = await userManager.FindByEmailAsync(email);
        if (technician is null)
        {
            technician = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = "Técnico de teste"
            };

            var result = await userManager.CreateAsync(technician, password);
            if (!result.Succeeded)
            {
                logger.LogError("Não foi possível criar o técnico: {Erros}",
                    string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }
        }

        // 4. Só atribui o papel se ainda não o tiver.
        if (!await userManager.IsInRoleAsync(technician, Roles.Technician))
        {
            await userManager.AddToRoleAsync(technician, Roles.Technician);
        }
    }
}