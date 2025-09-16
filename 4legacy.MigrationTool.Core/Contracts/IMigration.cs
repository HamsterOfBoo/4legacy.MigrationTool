namespace _4legacy.MigrationTool.Core.Contracts;

public interface IMigration
{
    /// <summary>
    /// Index for migration sorting
    /// </summary>
    long Timestamp { get; }

    /// <summary>
    /// Apply migration
    /// </summary>
    Task Up();

    /// <summary>
    /// Revert migration
    /// </summary>
    Task Down();
}