using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Alerts;
using OpenAlprWebhookProcessor.Data;
using System.Data.Common;

namespace Tests.Alerts.GetAlerts
{
    public class GetAlertsTests
    {
        private ProcessorContext _context;
        private DbContextOptions<ProcessorContext> _dbContextOptions;
        private DbConnection _connection;

        [SetUp]
        public async Task SetupAsync()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            await _connection.OpenAsync();

            _dbContextOptions = new DbContextOptionsBuilder<ProcessorContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ProcessorContext(_dbContextOptions);
            await _context.Database.EnsureCreatedAsync();

            _context.Alerts.AddRange(
                new OpenAlprWebhookProcessor.Data.Alert { Id = Guid.NewGuid(), PlateNumber = "ABC123", IsStrictMatch = true, Description = "Stolen vehicle" },
                new OpenAlprWebhookProcessor.Data.Alert { Id = Guid.NewGuid(), PlateNumber = "XYZ789", IsStrictMatch = false, Description = "Suspicious activity" }
            );
            await _context.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }

        [Test]
        public async Task HandleAsync_ReturnsAllAlerts()
        {
            // Arrange
            var handler = new GetAlertsRequestHandler(_context);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.HandleAsync(cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
        }
    }
}