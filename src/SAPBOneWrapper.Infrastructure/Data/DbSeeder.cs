using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SAPBOneWrapper.Domain.Entities;
using SAPBOneWrapper.Domain.Enums;
using SAPBOneWrapper.Infrastructure.Identity;

namespace SAPBOneWrapper.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
        await SeedBusinessPartnersAsync(context);
    }

    // -------------------------------------------------------------------------
    // Roles
    // -------------------------------------------------------------------------
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Manager", "User"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // -------------------------------------------------------------------------
    // Admin user
    // -------------------------------------------------------------------------
    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string email = "admin@gmail.com";

        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = "System Administrator",
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, "Manager1!");

        if (result.Succeeded)
            await userManager.AddToRolesAsync(admin, ["Admin", "Manager", "User"]);
    }

    // -------------------------------------------------------------------------
    // Sample business partners
    // -------------------------------------------------------------------------
    private static async Task SeedBusinessPartnersAsync(AppDbContext context)
    {
        if (await context.BusinessPartners.AnyAsync())
            return;

        var partners = new List<BusinessPartner>
        {
            new()
            {
                CardCode = "C00001",
                CardName = "Acme Corporation",
                CardType = CardType.Customer,
                Phone = "+1-555-0101",
                Email = "contact@acme.com",
                Address = "123 Main St",
                City = "New York",
                Country = "US",
                PostCode = "10001",
                Currency = "USD",
                CreditLimit = 50000m,
                Active = true,
                SyncStatus = SyncStatus.PendingCreate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                CardCode = "C00002",
                CardName = "GlobalTech Ltd",
                CardType = CardType.Customer,
                Phone = "+44-20-7946-0958",
                Email = "info@globaltech.co.uk",
                Address = "10 Baker Street",
                City = "London",
                Country = "GB",
                PostCode = "W1U 8ED",
                Currency = "GBP",
                CreditLimit = 30000m,
                Active = true,
                SyncStatus = SyncStatus.PendingCreate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                CardCode = "S00001",
                CardName = "Alpha Supplies Inc",
                CardType = CardType.Supplier,
                Phone = "+1-555-0202",
                Email = "orders@alphasupplies.com",
                Address = "456 Industrial Ave",
                City = "Chicago",
                Country = "US",
                PostCode = "60601",
                Currency = "USD",
                Active = true,
                SyncStatus = SyncStatus.PendingCreate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                CardCode = "S00002",
                CardName = "Euro Parts GmbH",
                CardType = CardType.Supplier,
                Phone = "+49-30-12345678",
                Email = "supply@europarts.de",
                Address = "Berliner Str. 99",
                City = "Berlin",
                Country = "DE",
                PostCode = "10115",
                Currency = "EUR",
                Active = true,
                SyncStatus = SyncStatus.PendingCreate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.BusinessPartners.AddRange(partners);
        await context.SaveChangesAsync();
    }
}
