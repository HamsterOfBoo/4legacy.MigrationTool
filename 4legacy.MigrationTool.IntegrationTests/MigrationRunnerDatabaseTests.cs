using System.Data;
using System.Reflection;
using _4legacy.MigrationTool.Core.Migrators;
using Microsoft.EntityFrameworkCore;

[assembly: LevelOfParallelism(1)] 
namespace _4legacy.MigrationTool.integrationTests;

[NonParallelizable]
public class MigrationRunnerDatabaseTests
{
    private DbContextOptions<MigrationToolDbContext> _dbOptions = new DbContextOptionsBuilder<MigrationToolDbContext>()
        .UseNpgsql("User ID=postgres;Password=123456;Server=localhost;Port=5432;Database=MigrationToolTestDb;")
        .Options;
    
    [SetUp]
    public void SetUp()
    {
    }

    [TearDown]
    public async Task TearDown()
    {
        await using var dbContext = new MigrationToolDbContext(_dbOptions);
        
        var dropTestTableQuery = @"DROP TABLE IF EXISTS public.""TestTable""";
        await dbContext.Database.ExecuteSqlRawAsync(dropTestTableQuery);
        
        var dropMigrationTableQuery = @"DROP TABLE IF EXISTS public.""4legacyMigrationToolIntegrationTestsMigrations""";
        await dbContext.Database.ExecuteSqlRawAsync(dropMigrationTableQuery);
    }

    [Test]
    public async Task MigrateDatabase_ThenMigrate_ShouldMigrate()
    {
        // arrange
        await using var dbContext = new MigrationToolDbContext(_dbOptions);
        var runner = new MigrationRunner(Assembly.GetExecutingAssembly());

        // act
        await runner.RunAsync(dbContext);

        // assert
        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        
        await using var checkTestTableExistingCommand = connection.CreateCommand();
        checkTestTableExistingCommand.CommandText = "SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TestTable'";
        var checkTestTableExistingResult = await checkTestTableExistingCommand.ExecuteScalarAsync();
        Assert.That(checkTestTableExistingResult, Is.Not.Null);
        
        await using var checkMigrationTableExistingCommand = connection.CreateCommand();
        checkTestTableExistingCommand.CommandText = "SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'TestTable'";
        var checkMigrationTableExistingResult = await checkTestTableExistingCommand.ExecuteScalarAsync();
        Assert.That(checkMigrationTableExistingResult, Is.Not.Null);
    }
}