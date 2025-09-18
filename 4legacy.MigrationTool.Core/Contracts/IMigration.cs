using Microsoft.EntityFrameworkCore;

namespace _4legacy.MigrationTool.Core.Contracts;

public interface IMigration
{
    /// <summary>
    /// Index for migration sorting 
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Apply migration
    /// </summary>
    Task Up(DbContext context);

    /// <summary>
    /// Revert migration
    /// </summary>
    Task Down(DbContext context);
}