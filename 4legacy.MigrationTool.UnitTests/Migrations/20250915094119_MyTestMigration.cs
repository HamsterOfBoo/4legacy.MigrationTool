using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using _4legacy.MigrationTool.Core.Contracts;

namespace SampleConsumerApp.Migrations
{
    public class MyTestMigration : IMigration
    {
        public long Timestamp => 1757929279L;
        private DbContext _dbContext;

        public async Task Up()
        {
            // TODO: Реализуйте логику накатывания миграции здесь.
            await Task.CompletedTask;
        }

        public async Task Down()
        {
            // TODO: Реализуйте логику накатывания миграции здесь.
            await Task.CompletedTask;
        }
    }
}