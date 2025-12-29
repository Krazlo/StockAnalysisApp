using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace UIApplication.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                await context.Database.MigrateAsync();
            }
            catch
            {
                // Database already exists or migration already applied
            }

            // ---- ROLE SEED ----
            var adminRole = "Admin";
            if (!await roleManager.Roles.AnyAsync(r => r.Name == adminRole))
                await roleManager.CreateAsync(new IdentityRole(adminRole));

            // ---- USER SEED ----
            var devEmail = "dev@stockapp.local";
            var user = await userManager.FindByEmailAsync(devEmail);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = devEmail,
                    Email = devEmail,
                    EmailConfirmed = true,
                    PromptsUsed = 0,
                    StocksAnalyzed = 0
                };

                var result = await userManager.CreateAsync(user, "DevUser123!");

                if (!result.Succeeded)
                    throw new Exception("Dev user creation failed: " +
                        string.Join(" | ", result.Errors.Select(e => e.Description)));
            }

            // ---- ASSIGN ROLE ----
            if (!await userManager.IsInRoleAsync(user, adminRole))
                await userManager.AddToRoleAsync(user, adminRole);
        }
    }
}
