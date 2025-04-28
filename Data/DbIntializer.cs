using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FileDigitilizationSystem.Models;

namespace FileDigitilizationSystem.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Ensure the database is created and migrations are applied.
            await context.Database.MigrateAsync();

            // Define the roles you intend to use.
            string[] roles = { "Admin", "Requester", "RecordsTeam" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed the Admin user if it doesn't exist.
            string adminEmail = "admin@stateland.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Chaplin",
                    LastName = "Maeresera",
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, "Test123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    // Log errors (or handle as needed)
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine(error.Description);
                    }
                }
            }
        }
    }
}
