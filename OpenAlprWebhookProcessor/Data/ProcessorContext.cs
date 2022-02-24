using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace OpenAlprWebhookProcessor.Data
{
    public class ProcessorContext : DbContext
    {
        public ProcessorContext(DbContextOptions<ProcessorContext> options)
            : base(options)
        {
        }

        public DbSet<PlateGroup> PlateGroups { get; set; }

        public DbSet<PlateGroupRaw> RawPlateGroups { get; set; }

        public DbSet<PlateGroupPossibleNumbers> PlateGroupPossibleNumbers { get; set; }

        public DbSet<Camera> Cameras { get; set; }

        public DbSet<Agent> Agents { get; set; }

        public DbSet<Ignore> Ignores { get; set; }

        public DbSet<Alert> Alerts { get; set; }

        public DbSet<WebhookForward> WebhookForwards { get; set; }

        public DbSet<Pushover> PushoverAlertClients { get; set; }

        public DbSet<Enricher> Enrichers { get; set; }

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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Debug);
    }
}
