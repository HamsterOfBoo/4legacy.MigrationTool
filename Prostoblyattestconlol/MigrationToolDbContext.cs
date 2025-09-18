using Microsoft.EntityFrameworkCore;

namespace Prostoblyattestconlol;

public class MigrationToolDbContext : DbContext
{
    public MigrationToolDbContext(DbContextOptions<MigrationToolDbContext> options) : base(options)
    {
    }
}