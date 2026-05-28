using Microsoft.AspNetCore.Identity;
using NotariaParroquial.Models;

namespace NotariaParroquial.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Administrador", "Operador", "Consulta"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        const string adminEmail = "admin@notaria.mx";
        const string adminPassword = "Admin123!";

        var existing = await userManager.FindByEmailAsync(adminEmail);
        if (existing == null)
        {
            var admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                NombreCompleto = "Administrador del Sistema",
                Rol = "Administrador",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Administrador");
        }
        else
        {
            // Ensure password is correct regardless of prior state
            var token = await userManager.GeneratePasswordResetTokenAsync(existing);
            await userManager.ResetPasswordAsync(existing, token, adminPassword);

            if (!await userManager.IsInRoleAsync(existing, "Administrador"))
                await userManager.AddToRoleAsync(existing, "Administrador");
        }
    }
}
