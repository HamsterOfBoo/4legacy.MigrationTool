using System.Reflection;
using _4legacy.MigrationTool.Core.Generators;

namespace _4legacy.MigrationTool.UnitTests;

[TestFixture]
public class Tests
{
    private MigrationGenerator _migrationGenerator;
    const string migrationName = "MyTestMigration";
    const string migrationsPath = "4legacy.MigrationTool.UnitTests\\Migrations";
    private static readonly string rootDir = Path.GetFullPath(Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
        @"..\..\..\.."));

    [SetUp]
    public void SetUp()
    {
        _migrationGenerator = new MigrationGenerator();
    }

    [TearDown]
    public void TearDown()
    {
        var migrationsDirectory = Path.Combine(rootDir, migrationsPath);
        
        foreach (string file in Directory.GetFiles(migrationsDirectory))
        {
            File.Delete(file);
        }
    }

    [Test]
    public async Task CreateMigrationFileAsync_ShouldCreateFile_WithCorrectNameFormat()
    {
        // Arrange

        // Act
        await _migrationGenerator.CreateMigrationFileAsync(migrationsPath, migrationName, "DbContext");

        // Assert
        var files = Directory.GetFiles(Path.Combine(rootDir, migrationsPath));
        Assert.That(files.Length, Is.EqualTo(1));
    }

}