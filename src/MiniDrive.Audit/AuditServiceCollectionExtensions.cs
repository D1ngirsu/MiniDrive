using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniDrive.Audit.Repositories;
using MiniDrive.Audit.Services;

namespace MiniDrive.Audit;

/// <summary>
/// Extension methods for registering audit services.
/// </summary>
public static class AuditServiceCollectionExtensions
{
    /// <summary>
    /// Adds audit logging services to the service collection.
    /// </summary>
    public static IServiceCollection AddAuditServices(this IServiceCollection services, IConfiguration? configuration = null, string? environmentName = null)
    {
        services.AddDbContext<AuditDbContext>(options =>
        {
            if (string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase("AuditDb");
            }
            else if (configuration != null)
            {
                var connectionString = configuration.GetConnectionString("AuditDb") 
                    ?? throw new InvalidOperationException("Connection string 'AuditDb' not found.");
                
                options.UseSqlServer(connectionString);
            }
            else
            {
                options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=MiniDrive_Audit;Trusted_Connection=True;TrustServerCertificate=True");
            }
        });

        services.AddScoped<AuditRepository>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}

