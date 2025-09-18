using Microsoft.EntityFrameworkCore;

namespace _4legacy.MigrationTool.integrationTests;

public class MigrationToolDbContext : DbContext
{
    public MigrationToolDbContext(DbContextOptions<MigrationToolDbContext> options) : base(options)
    {
    }
}