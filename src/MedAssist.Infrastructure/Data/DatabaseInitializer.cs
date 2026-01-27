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

        await EnsurePersonalDataColumnsAsync(dbContext, logger, cancellationToken);
        await EnsureSpecializationColumnsAsync(dbContext, logger, cancellationToken);
        await EnsureTelegramUserIdColumnAsync(dbContext, logger, cancellationToken);
        await EnsureTelegramUsernameColumnRemovedAsync(dbContext, logger, cancellationToken);
        await EnsureDoctorPatientForeignKeyAsync(dbContext, logger, cancellationToken);
        await EnsureDoctorLastSelectedPatientForeignKeyAsync(dbContext, logger, cancellationToken);
        await EnsureRegistrationNicknameColumnAsync(dbContext, logger, cancellationToken);
        await EnsureRegistrationConfirmedColumnRemovedAsync(dbContext, logger, cancellationToken);
        await EnsureDoctorProfileColumnsRemovedAsync(dbContext, logger, cancellationToken);
        await EnsureStaticContentTableAsync(dbContext, logger, cancellationToken);
    }

    private static async Task EnsureSpecializationColumnsAsync(
        MedAssistDbContext dbContext,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            ALTER TABLE IF EXISTS ""Doctors""
            DROP COLUMN IF EXISTS ""SpecializationCode"",
            DROP COLUMN IF EXISTS ""SpecializationTitle"";

            ALTER TABLE IF EXISTS ""Doctors""
            ADD COLUMN IF NOT EXISTS ""SpecializationCodes"" text[] DEFAULT ARRAY[]::text[],
            ADD COLUMN IF NOT EXISTS ""SpecializationTitles"" text[] DEFAULT ARRAY[]::text[],
            ADD COLUMN IF NOT EXISTS ""Registration_SpecializationCodes"" text[] DEFAULT ARRAY[]::text[],
            ADD COLUMN IF NOT EXISTS ""Registration_SpecializationTitles"" text[] DEFAULT ARRAY[]::text[];

            UPDATE ""Doctors"" SET ""SpecializationCodes"" = ARRAY[]::text[] WHERE ""SpecializationCodes"" IS NULL;
            UPDATE ""Doctors"" SET ""SpecializationTitles"" = ARRAY[]::text[] WHERE ""SpecializationTitles"" IS NULL;
            UPDATE ""Doctors"" SET ""Registration_SpecializationCodes"" = ARRAY[]::text[] WHERE ""Registration_SpecializationCodes"" IS NULL;
            UPDATE ""Doctors"" SET ""Registration_SpecializationTitles"" = ARRAY[]::text[] WHERE ""Registration_SpecializationTitles"" IS NULL;
        ";

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        logger?.LogInformation("Ensured specialization columns exist in Doctors table.");
    }

    private static async Task EnsureStaticContentTableAsync(
        MedAssistDbContext dbContext,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS ""StaticContents"" (
                ""Id"" uuid NOT NULL,
                ""Code"" text NOT NULL,
                ""Title"" text NULL,
                ""Value"" text NOT NULL,
                ""UpdatedAt"" timestamp with time zone NOT NULL,
                CONSTRAINT ""PK_StaticContents"" PRIMARY KEY (""Id"")
            );

            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_StaticContents_Code"" ON ""StaticContents"" (""Code"");

            INSERT INTO ""StaticContents"" (""Id"", ""Code"", ""Title"", ""Value"", ""UpdatedAt"")
            VALUES
                (gen_random_uuid(), 'eula', 'EULA', 'TODO: eula', now()),
                (gen_random_uuid(), 'help', 'Help', 'TODO: help', now())
            ON CONFLICT (""Code"") DO NOTHING;
        ";

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        logger?.LogInformation("Ensured StaticContents table exists.");
    }

    private static async Task EnsurePersonalDataColumnsAsync(
        MedAssistDbContext dbContext,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            ALTER TABLE IF EXISTS ""Patients""
            ADD COLUMN IF NOT EXISTS ""AgeYears"" integer NULL,
            ADD COLUMN IF NOT EXISTS ""Nickname"" text NULL;

            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = 'Patients'
                      AND column_name = 'BirthDate'
                ) THEN
                    UPDATE ""Patients""
                    SET ""AgeYears"" = date_part('year', age(""BirthDate""))::int
                    WHERE ""AgeYears"" IS NULL
                      AND ""BirthDate"" IS NOT NULL;
                END IF;
            END $$;

            ALTER TABLE IF EXISTS ""Patients""
            DROP COLUMN IF EXISTS ""FullName"",
            DROP COLUMN IF EXISTS ""BirthDate"",
            DROP COLUMN IF EXISTS ""Phone"",
            DROP COLUMN IF EXISTS ""Email"";

            ALTER TABLE IF EXISTS ""Doctors""
            DROP COLUMN IF EXISTS ""DisplayName"";
        ";

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        logger?.LogInformation("Ensured personal data columns are removed.");
    }

    private static async Task EnsureTelegramUserIdColumnAsync(
        MedAssistDbContext dbContext,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            ALTER TABLE IF EXISTS ""Doctors""
            ADD COLUMN IF NOT EXISTS ""TelegramUserId"" bigint NULL;

            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Doctors_TelegramUserId""
            ON ""Doctors"" (""TelegramUserId"");
        ";

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        logger?.LogInformation("Ensured TelegramUserId column exists in Doctors table.");
    }

    private static async Task EnsureRegistrationNicknameColumnAsync(
        MedAssistDbContext dbContext,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            ALTER TABLE IF EXISTS ""Doctors""
            ADD COLUMN IF NOT EXISTS ""Registration_Nickname"" text NULL;
        ";

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        logger?.LogInformation("Ensured Registration_Nickname column exists in Doctors table.");
    }

    private static async Task EnsureTelegramUsernameColumnRemovedAsync(
        MedAssistDbContext dbContext,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            ALTER TABLE IF EXISTS ""Doctors""
            DROP COLUMN IF EXISTS ""TelegramUsername"";
        ";

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        logger?.LogInformation("Removed TelegramUsername column from Doctors table.");
    }

    private static async Task EnsureDoctorPatientForeignKeyAsync(
        MedAssistDbContext dbContext,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            DELETE FROM ""Patients"" p
            WHERE NOT EXISTS (
                SELECT 1 FROM ""Doctors"" d WHERE d.""Id"" = p.""DoctorId""
            );

            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM pg_constraint
                    WHERE conname = 'FK_Patients_Doctors_DoctorId'
                ) THEN
                    ALTER TABLE ""Patients""
                    ADD CONSTRAINT ""FK_Patients_Doctors_DoctorId""
                    FOREIGN KEY (""DoctorId"") REFERENCES ""Doctors"" (""Id"")
                    ON DELETE CASCADE;
                END IF;
            END $$;
        ";

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        logger?.LogInformation("Ensured FK Patients.DoctorId -> Doctors.Id with cascade delete.");
    }

    private static async Task EnsureDoctorLastSelectedPatientForeignKeyAsync(
        MedAssistDbContext dbContext,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE ""Doctors"" d
            SET ""LastSelectedPatientId"" = NULL
            WHERE ""LastSelectedPatientId"" IS NOT NULL
              AND NOT EXISTS (
                  SELECT 1 FROM ""Patients"" p WHERE p.""Id"" = d.""LastSelectedPatientId""
              );

            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM pg_constraint
                    WHERE conname = 'FK_Doctors_LastSelectedPatientId'
                ) THEN
                    ALTER TABLE ""Doctors""
                    ADD CONSTRAINT ""FK_Doctors_LastSelectedPatientId""
                    FOREIGN KEY (""LastSelectedPatientId"") REFERENCES ""Patients"" (""Id"")
                    ON DELETE SET NULL;
                END IF;
            END $$;
        ";

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        logger?.LogInformation("Ensured FK Doctors.LastSelectedPatientId -> Patients.Id with ON DELETE SET NULL.");
    }

    private static async Task EnsureRegistrationConfirmedColumnRemovedAsync(
        MedAssistDbContext dbContext,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            ALTER TABLE IF EXISTS ""Doctors""
            DROP COLUMN IF EXISTS ""Registration_Confirmed"",
            DROP COLUMN IF EXISTS ""Registration_HumanInLoopConfirmed"";
        ";

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        logger?.LogInformation("Removed registration confirmation columns from Doctors table.");
    }

    private static async Task EnsureDoctorProfileColumnsRemovedAsync(
        MedAssistDbContext dbContext,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            ALTER TABLE IF EXISTS ""Doctors""
            DROP COLUMN IF EXISTS ""Degrees"",
            DROP COLUMN IF EXISTS ""ExperienceYears"",
            DROP COLUMN IF EXISTS ""Languages"",
            DROP COLUMN IF EXISTS ""Bio"",
            DROP COLUMN IF EXISTS ""FocusAreas"",
            DROP COLUMN IF EXISTS ""AcceptingNewPatients"",
            DROP COLUMN IF EXISTS ""Location"",
            DROP COLUMN IF EXISTS ""ContactPolicy"",
            DROP COLUMN IF EXISTS ""AvatarUrl"",
            DROP COLUMN IF EXISTS ""Rating"";
        ";

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        logger?.LogInformation("Removed extended doctor profile columns from Doctors table.");
    }
}
