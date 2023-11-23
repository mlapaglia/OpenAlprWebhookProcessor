using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;

namespace Tests
{
    public class EfContextCreator
    {
        readonly SqliteConnection _connection;

        readonly DbContextOptions<ProcessorContext> _contextOptions;

        public EfContextCreator()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            _contextOptions = new DbContextOptionsBuilder<ProcessorContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = new ProcessorContext(_contextOptions);

            context.Database.EnsureCreated();
        }

        public ProcessorContext CreateContext() => new(_contextOptions);

        public void Dispose() => _connection.Dispose();
    }
}
