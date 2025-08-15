using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Data
{
    public class ProcessorContext : DbContext
    {
        public ProcessorContext(DbContextOptions<ProcessorContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await EnsureWalModeAsync(cancellationToken);
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            EnsureWalMode();
            return base.SaveChanges();
        }

        private async Task EnsureWalModeAsync(CancellationToken cancellationToken = default)
        {
            if (Database.GetDbConnection().State != System.Data.ConnectionState.Open)
            {
                await Database.OpenConnectionAsync(cancellationToken);
            }
            
            try
            {
                await Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL; PRAGMA busy_timeout=30000;", cancellationToken);
            }
            catch
            {
                // Ignore errors if WAL mode is already set or not supported
            }
        }

        private void EnsureWalMode()
        {
            if (Database.GetDbConnection().State != System.Data.ConnectionState.Open)
            {
                Database.OpenConnection();
            }
            
            try
            {
                Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL; PRAGMA busy_timeout=30000;");
            }
            catch
            {
                // Ignore errors if WAL mode is already set or not supported
            }
        }

        public DbSet<PlateGroup> PlateGroups { get; set; }

        public DbSet<PlateGroupRaw> RawPlateGroups { get; set; }

        public DbSet<PlateGroupPossibleNumbers> PlateGroupPossibleNumbers { get; set; }

        public DbSet<Camera> Cameras { get; set; }

        public DbSet<CameraMask> CameraMasks { get; set; }

        public DbSet<Agent> Agents { get; set; }

        public DbSet<Ignore> Ignores { get; set; }

        public DbSet<Alert> Alerts { get; set; }

        public DbSet<WebhookForward> WebhookForwards { get; set; }

        public DbSet<Pushover> PushoverAlertClients { get; set; }

        public DbSet<Enricher> Enrichers { get; set; }

        public DbSet<PlateImage> PlateImages { get; set; }

        public DbSet<PlateImage> VehicleImages { get; set; }

        public DbSet<WebPushSubscription> WebPushSubscriptions { get; set; }

        public DbSet<WebPushSubscriptionKey> WebPushSubscriptionKeys { get; set; }

        public DbSet<WebPushSettings> WebPushSettings { get; set; }
        
        public DbSet<MachineLearningConfiguration> MachineLearningConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var method = entityType
                    .ClrType
                    .GetMethod("OnModelCreating", BindingFlags.Static | BindingFlags.Public);

                method?.Invoke(null, new object[] { modelBuilder });
            }
        }
    }
}
