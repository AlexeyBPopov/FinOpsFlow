using FinOpsFlow.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinOpsFlow.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var context = sp.GetRequiredService<ApplicationDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedCategoriesAsync(context);
        await SeedUsersAsync(userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Manager", "OperationsUser", "Auditor"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        context.Categories.AddRange(
            new Category { Name = "Trade Operations", Description = "Trade-related requests" },
            new Category { Name = "Compliance", Description = "Regulatory and compliance requests" },
            new Category { Name = "Finance", Description = "Financial operations requests" },
            new Category { Name = "IT Support", Description = "IT-related requests" },
            new Category { Name = "General", Description = "General requests" }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        await CreateUser(userManager, "admin@finopsflow.com", "Admin", "User", "Admin123!", "Admin");
        await CreateUser(userManager, "manager@finopsflow.com", "Manager", "Smith", "Manager123!", "Manager");
        await CreateUser(userManager, "ops@finopsflow.com", "Operations", "Jones", "Ops123!", "OperationsUser");
        await CreateUser(userManager, "auditor@finopsflow.com", "Auditor", "Brown", "Auditor123!", "Auditor");
    }

    private static async Task CreateUser(
        UserManager<ApplicationUser> userManager,
        string email, string firstName, string lastName,
        string password, string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null) return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
    }
}