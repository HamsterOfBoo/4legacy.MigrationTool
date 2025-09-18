using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using _4legacy.MigrationTool.Core.Contracts;

namespace _4legacy.MigrationTool.Migrations;

public class MyTestMigration : IMigration
{
    public DateTimeOffset Timestamp => DateTimeOffset.UtcNow;

    public async Task Up(DbContext context)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        
        var query = @"CREATE TABLE IF NOT EXISTS public.""TestTable""
                (
                    id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
                    justfloat double precision,
                    CONSTRAINT ""TestTable_pkey"" PRIMARY KEY (id)
                )";
            
        await context.Database.ExecuteSqlRawAsync(query);
    }

    public async Task Down(DbContext context)
    {
        // TODO: Реализуйте логику накатывания миграции здесь.
        await Task.CompletedTask;
    }
}