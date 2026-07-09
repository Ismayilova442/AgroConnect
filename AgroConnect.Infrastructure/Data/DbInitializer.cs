using AgroConnect.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AgroConnect.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "SuperAdmin", "Farmer", "Member" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            string adminEmail = "admin@agroconnect.az";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Super",
                    LastName = "Admin",
                    EmailConfirmed = true
                };

                var createPowerUser = await userManager.CreateAsync(newAdmin, "Admin123!");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "SuperAdmin");
                }
            }

            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Arıçılıq", IconClass = "bi-bug" },
                    new Category { Name = "Maldarlıq", IconClass = "bi-shop" },
                    new Category { Name = "Meyvəçilik", IconClass = "bi-apple" },
                    new Category { Name = "Tərəvəzçilik", IconClass = "bi-flower1" },
                    new Category { Name = "Əkinçilik", IconClass = "bi-tree" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
