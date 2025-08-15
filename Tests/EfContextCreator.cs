using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.Users.Data;

namespace Tests
{
    public class EfContextCreator
    {
        readonly SqliteConnection _connection;
        readonly SqliteConnection _usersConnection;

        readonly DbContextOptions<ProcessorContext> _contextOptions;
        readonly DbContextOptions<UsersContext> _usersContextOptions;

        public EfContextCreator()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            _contextOptions = new DbContextOptionsBuilder<ProcessorContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = new ProcessorContext(_contextOptions);
            context.Database.EnsureCreated();

            // Users context setup
            _usersConnection = new SqliteConnection("Filename=:memory:");
            _usersConnection.Open();

            _usersContextOptions = new DbContextOptionsBuilder<UsersContext>()
                .UseSqlite(_usersConnection)
                .Options;

            using var usersContext = new UsersContext(_usersContextOptions);
            usersContext.Database.EnsureCreated();
        }

        public ProcessorContext CreateContext() => new(_contextOptions);
        
        public UsersContext CreateUsersContext() => new(_usersContextOptions);

        public void Dispose() 
        { 
            _connection.Dispose();
            _usersConnection.Dispose();
        }
    }
}
