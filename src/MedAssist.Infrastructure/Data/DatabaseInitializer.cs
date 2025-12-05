using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MedAssist.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task EnsureDatabaseAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetService<ILoggerFactory>()?.CreateLogger("DatabaseInitializer");
        var dbContext = provider.GetRequiredService<MedAssistDbContext>();
        var connectionString = dbContext.Database.GetConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string is not configured.");
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;

        // Подключаемся к системной базе, чтобы создать целевую, если её нет.
        var adminBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "postgres"
        };

        await using (var adminConnection = new NpgsqlConnection(adminBuilder.ConnectionString))
        {
            await adminConnection.OpenAsync(cancellationToken);
            await using var cmd = adminConnection.CreateCommand();
            cmd.CommandText = $"SELECT 1 FROM pg_database WHERE datname = @name;";
            cmd.Parameters.AddWithValue("name", databaseName ?? string.Empty);
            var exists = await cmd.ExecuteScalarAsync(cancellationToken) != null;
            if (!exists)
            {
                logger?.LogInformation("Создаю базу данных {Database}", databaseName);
                await using var createCmd = adminConnection.CreateCommand();
                createCmd.CommandText = $"CREATE DATABASE \"{databaseName}\"";
                await createCmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        await EnsureSpecializationColumnsAsync(dbContext, logger, cancellationToken);
    }

    private static async Task EnsureSpecializationColumnsAsync(
        MedAssistDbContext dbContext,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            ALTER TABLE IF EXISTS ""Doctors""
            ADD COLUMN IF NOT EXISTS ""SpecializationCodes"" text[] DEFAULT '{}'::text[],
            ADD COLUMN IF NOT EXISTS ""SpecializationTitles"" text[] DEFAULT '{}'::text[],
            ADD COLUMN IF NOT EXISTS ""Registration_SpecializationCodes"" text[] DEFAULT '{}'::text[],
            ADD COLUMN IF NOT EXISTS ""Registration_SpecializationTitles"" text[] DEFAULT '{}'::text[];

            UPDATE ""Doctors"" SET ""SpecializationCodes"" = '{}' WHERE ""SpecializationCodes"" IS NULL;
            UPDATE ""Doctors"" SET ""SpecializationTitles"" = '{}' WHERE ""SpecializationTitles"" IS NULL;
            UPDATE ""Doctors"" SET ""Registration_SpecializationCodes"" = '{}' WHERE ""Registration_SpecializationCodes"" IS NULL;
            UPDATE ""Doctors"" SET ""Registration_SpecializationTitles"" = '{}' WHERE ""Registration_SpecializationTitles"" IS NULL;
        ";

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        logger?.LogInformation("Ensured specialization columns exist in Doctors table.");
    }
}
