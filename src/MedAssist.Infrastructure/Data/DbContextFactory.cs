using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedAssist.Infrastructure.Data;

public static class DbContextFactory
{
    public static IServiceCollection AddMedAssistDb(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Default must be configured for Postgres.");
        }

        services.AddDbContext<MedAssistDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
