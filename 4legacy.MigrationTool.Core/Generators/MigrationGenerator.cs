using System.Reflection;

namespace _4legacy.MigrationTool.Core.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

public class MigrationGenerator
{
    /// <summary>
    /// Creates a file with empty migration
    /// </summary>
    /// <param name="migrationName">Migration name</param>
    /// <param name="migrationFilePath">Path to data access project with migrations folder. EG: "4legacy.DataAccess\Migrations"</param>
    /// <param name="dbContextIdentifier">DbContextName</param>
    public async Task CreateMigrationFileAsync(string migrationFilePath, string migrationName, string dbContextIdentifier)
    {
        var timestamp = DateTimeOffset.UtcNow;
        var fileName = $"{timestamp.ToString("yyyyMMddHHmmss")}_{migrationName}.cs";
        var className = $"{migrationName}";

        var usings = new[] { "System.Threading.Tasks", "Microsoft.EntityFrameworkCore", "_4legacy.MigrationTool.Core.Contracts" };

        var namespaceDeclaration = NamespaceDeclaration(IdentifierName("_4legacy.MigrationTool.Migrations"));

        var classDeclaration = ClassDeclaration(className)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(SimpleBaseType(IdentifierName("IMigration")));

        var timestampProperty = PropertyDeclaration(
                IdentifierName("DateTimeOffset"),
                "Timestamp")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithExpressionBody(
                ArrowExpressionClause(
                    MemberAccessExpression( // Создает 'DateTime.UtcNow'
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("DateTimeOffset"),
                        IdentifierName("UtcNow")
                    )
                )
            )
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        
        var emptyMethodBodyBlock = Block(
            ExpressionStatement(
                AwaitExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Task"),
                        IdentifierName("CompletedTask")
                    )
                )
            ).WithLeadingTrivia(
                TriviaList(
                    Comment("// TODO: Реализуйте логику накатывания миграции здесь."),
                    CarriageReturnLineFeed
                ))
        );
        
        var upMethod = MethodDeclaration(ParseTypeName("Task"), 
                Identifier("Up"))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("context")).WithType(IdentifierName("DbContext"))
            )
            .WithBody(emptyMethodBodyBlock);

        var downMethod = MethodDeclaration(ParseTypeName("Task"), 
                Identifier("Down"))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("context")).WithType(IdentifierName("DbContext"))
            )
            .WithBody(emptyMethodBodyBlock);

        classDeclaration = classDeclaration.AddMembers(timestampProperty, upMethod, downMethod);
        
        var code = CompilationUnit()
            .AddUsings(usings.Select(u => UsingDirective(IdentifierName(u))).ToArray())
            .AddMembers(namespaceDeclaration.AddMembers(classDeclaration))
            .NormalizeWhitespace()
            .ToFullString();
        
        var rootPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\..\.."));
        
        var migrationsDirectory = Path.Combine(rootPath, migrationFilePath);
        Directory.CreateDirectory(migrationsDirectory);
        var filePath = Path.Combine(migrationsDirectory, fileName);
        await File.WriteAllTextAsync(filePath, code);
        
        Console.WriteLine($"Migration file successfully created: {filePath}");
    }
}