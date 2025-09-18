using System.Data;
using System.Reflection;
using _4legacy.MigrationTool.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace _4legacy.MigrationTool.Core.Migrators;

public enum MigrationDirection
{
    Up,
    Down
}

public class MigrationRunner
{
    private readonly Assembly _migrationsAssembly;

    public MigrationRunner(Assembly migrationsAssembly)
    {
        _migrationsAssembly = migrationsAssembly;
    }

    public async Task RunAsync(DbContext dbContext, string? targetMigration = null)
    {
        Console.WriteLine($"Migrator is running for {dbContext.Database.GetDbConnection().Database}");


        var escapedAssemblyName = Assembly.GetAssembly(dbContext.GetType()).GetName().Name.Replace(".", String.Empty);
        var migrationTableName = $"{escapedAssemblyName}Migrations";


        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            await EnsureHistoryTableExistsAsync(dbContext, migrationTableName);
            
            var allMigrations = _migrationsAssembly.GetTypes()
                .Where(t => typeof(IMigration).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(t => (IMigration)Activator.CreateInstance(t)!)
                .OrderBy(m => m.Timestamp)
                .ToList();

            var appliedMigrationNames = await GetAppliedMigrationNamesAsync(dbContext, migrationTableName);

            List<IMigration> migrationsToRun;

            // if (!string.IsNullOrEmpty(targetMigration))
            // {
            //     if (appliedMigrationNames.Contains(targetMigration))
            //     {
            //         Console.WriteLine($"Target migration {targetMigration} is already applied");
            //         await transaction.CommitAsync();
            //         return;
            //     }
            //
            //     var targetMigrationIndex = allMigrations
            //         .Select(x => x.GetType().Name)
            //         .ToList()
            //         .IndexOf(targetMigration);
            //
            //     var migrationCountToSkip = allMigrations.Count - targetMigrationIndex;
            //
            //     migrationsToRun = allMigrations
            //         .Where(m => !appliedMigrationNames.Contains(m.GetType().Name))
            //         .Take(migrationCountToSkip)
            //         .ToList();
            // }
            // else
            {
                migrationsToRun = allMigrations
                    .Where(m => !appliedMigrationNames.Contains(m.GetType().Name))
                    .ToList();
            }

            Console.WriteLine($"{migrationsToRun.Count} migrations founded to apply.");

            if (!migrationsToRun.Any())
            {
                Console.WriteLine("No migrations for applying.");
                await transaction.CommitAsync();
                return;
            }

            Console.WriteLine($"{migrationsToRun.Count} migration will be applying.");

            foreach (var migration in migrationsToRun)
            {
                var migrationName = migration.GetType().Name;
                Console.WriteLine($"Applying {migrationName}");
                await migration.Up(dbContext);
                await AddMigrationToHistoryAsync(dbContext, migrationTableName, migrationName, migration.Timestamp);
            }

            await transaction.CommitAsync();

            Console.WriteLine("All migrations applied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error: {ex.Message}. Migration rollback...");

            await transaction.RollbackAsync();

            Console.WriteLine("Rollback successfully complete.");
            throw;
        }
    }

    private async Task EnsureHistoryTableExistsAsync(DbContext context, string migrationTableName)
    {
        var providerName = context.Database.ProviderName!;
        string checkTableSql;
        string createTableSql;

        if (providerName.Contains("SqlServer"))
        {
            checkTableSql = $"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{migrationTableName}'";
            createTableSql = $@"CREATE TABLE [{migrationTableName}] (
                    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [MigrationName] NVARCHAR(255) NOT NULL,
                    [Timestamp] datetime NOT NULL,
                );";
        }
        else if (providerName.Contains("Npgsql"))
        {
            checkTableSql =
                $"SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '{migrationTableName}'";
            createTableSql = $@"CREATE TABLE public.""{migrationTableName}"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""MigrationName"" VARCHAR(255) NOT NULL,
                    ""Timestamp"" TIMESTAMP NOT NULL
                );";
        }
        else if (providerName.Contains("Sqlite"))
        {
            checkTableSql = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{migrationTableName}'";
            createTableSql = $@"CREATE TABLE ""{migrationTableName}"" (
                    ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    ""MigrationName"" TEXT NOT NULL
                );";
        }
        else
        {
            throw new NotSupportedException($"'{providerName}' is not supported.");
        }

        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        await using var command = connection.CreateCommand();
        command.CommandText = checkTableSql;

        var result = await command.ExecuteScalarAsync();

        if (result == null)
        {
            Console.WriteLine($"Table '{migrationTableName}' isn't found. Creating new one...");
            await context.Database.ExecuteSqlRawAsync(createTableSql);
            Console.WriteLine("Migration history table created.");
        }
        
    }

    private async Task<List<string>> GetAppliedMigrationNamesAsync(DbContext context, string migrationTableName)
    {
        var names = new List<string>();
        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT \"MigrationName\" FROM public.\"{migrationTableName}\"";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }

        return names.OrderByDescending(x => x).ToList();
    }

    private async Task AddMigrationToHistoryAsync(DbContext context, string migrationTableName, string migrationName, DateTimeOffset timestamp)
    {
        await context.Database.ExecuteSqlRawAsync($"INSERT INTO \"{migrationTableName}\" (\"MigrationName\",\"Timestamp\") VALUES ({{0}}, {{1}})",
            migrationName, timestamp);
    }

    private async Task RemoveMigrationFromHistoryAsync(DbContext context, string migrationTableName,
        string migrationName)
    {
        await context.Database.ExecuteSqlRawAsync($"DELETE FROM {migrationTableName} WHERE MigrationName = {{0}}",
            migrationName);
    }
}