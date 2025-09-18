using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;
using _4legacy.MigrationTool.Core;
using _4legacy.MigrationTool.Core.Generators;

// Create migration region
var createCommand = new Command("CreateMigration", "Creates a new migration.");

var dbNameOption = new Option<string>("--connectionString", "Database connection string.") { Required = true };
var migrationNameOption = new Option<string>("--migration-name", "Migration name") { Required = true };

createCommand.Options.Add(dbNameOption);
createCommand.Options.Add(migrationNameOption);

createCommand.SetAction(async x =>
{
    var migrationName = x.GetValue(migrationNameOption);
    var dbContextName = x.GetValue(dbNameOption);
    Console.WriteLine($"Migration '{x.GetValue(migrationNameOption)}' is creating for '{dbContextName}'...");
    var generator = new MigrationGenerator();
    await generator.CreateMigrationFileAsync("migrationPath", migrationName, dbContextName );
});


// Run migration region
var runCommand = new Command("RunMigration", "Run migration.");

var directionOption = new Option<string>("--target", "Migration target file to run.");

runCommand.Options.Add(dbNameOption);
runCommand.Options.Add(directionOption);

createCommand.SetAction(async x =>
{
    var migrationName = x.GetValue(migrationNameOption);
    var dbContextName = x.GetValue(dbNameOption);
    Console.WriteLine($"Migration '{x.GetValue(migrationNameOption)}' is creating for '{dbContextName}'...");
    var generator = new MigrationGenerator();
    await generator.CreateMigrationFileAsync("migrationPath", migrationName, dbContextName );
});
//
// runCommand.SetHandler(async (dbName, direction) =>
// {
//     try
//     {
//         // Находим сборку текущего проекта, откуда запускается инструмент
//         var entryAssembly = Assembly.GetEntryAssembly();
//         if (entryAssembly == null)
//         {
//             Console.Error.WriteLine("Не удалось определить сборку для поиска миграций.");
//             return;
//         }
//
//         // Ищем реализацию IDbContextFactory в этой сборке
//         var factoryType = entryAssembly.GetTypes()
//             .FirstOrDefault(t => typeof(IDbContextFactory).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
//
//         if (factoryType == null)
//         {
//             Console.Error.WriteLine($"Не найдена реализация интерфейса '{nameof(IDbContextFactory)}' в сборке '{entryAssembly.FullName}'.");
//             return;
//         }
//
//         var contextFactory = (IDbContextFactory)Activator.CreateInstance(factoryType)!;
//
//         var runner = new MigrationRunner(contextFactory, entryAssembly);
//         await runner.RunAsync(dbName, direction);
//     }
//     catch (Exception ex)
//     {
//         Console.ForegroundColor = ConsoleColor.Red;
//         Console.WriteLine($"Критическая ошибка: {ex.Message}");
//         Console.ResetColor();
//     }
//
// }, dbNameOption, directionOption);
//
//
// // --- Корневая команда ---
// var rootCommand = new RootCommand("Инструмент для работы с миграциями");
// rootCommand.AddCommand(createCommand);
// rootCommand.AddCommand(runCommand);
//
// return await rootCommand.InvokeAsync(args);
