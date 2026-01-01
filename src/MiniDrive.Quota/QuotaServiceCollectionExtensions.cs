using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniDrive.Quota.Repositories;
using MiniDrive.Quota.Services;

namespace MiniDrive.Quota;

/// <summary>
/// Extension methods for registering quota services.
/// </summary>
public static class QuotaServiceCollectionExtensions
{
    /// <summary>
    /// Adds quota management services to the service collection.
    /// </summary>
    public static IServiceCollection AddQuotaServices(this IServiceCollection services, IConfiguration? configuration = null, string? environmentName = null)
    {
        services.AddDbContext<QuotaDbContext>(options =>
        {
            if (string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase("QuotaDb");
            }
            else if (configuration != null)
            {
                var connectionString = configuration.GetConnectionString("QuotaDb") 
                    ?? throw new InvalidOperationException("Connection string 'QuotaDb' not found.");
                
                options.UseSqlServer(connectionString);
            }
            else
            {
                options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=MiniDrive_Quota;Trusted_Connection=True;TrustServerCertificate=True");
            }
        });

        services.AddScoped<QuotaRepository>();
        services.AddScoped<IQuotaService, QuotaService>();

        return services;
    }
}

