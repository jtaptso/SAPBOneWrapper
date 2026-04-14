using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SAPBOneWrapper.Domain.Interfaces;
using SAPBOneWrapper.Infrastructure.Data;
using SAPBOneWrapper.Infrastructure.Identity;
using SAPBOneWrapper.Infrastructure.Repositories;
using SAPBOneWrapper.Infrastructure.SapServiceLayer;

namespace SAPBOneWrapper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core + SQL Server
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // ASP.NET Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Repositories
        services.AddScoped<IBusinessPartnerRepository, BusinessPartnerRepository>();

        // SAP B1 Service Layer
        services.Configure<SapB1Options>(configuration.GetSection(SapB1Options.SectionName));
        services.AddTransient<SapB1SessionHandler>();

        services.AddHttpClient<ISapB1ServiceLayerClient, SapB1ServiceLayerClient>(client =>
            {
                var sapConfig = configuration.GetSection(SapB1Options.SectionName).Get<SapB1Options>()!;
                client.BaseAddress = new Uri(sapConfig.ServiceLayerUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddHttpMessageHandler<SapB1SessionHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // SAP B1 Service Layer often uses self-signed certificates
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

        return services;
    }
}
