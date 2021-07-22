using Microsoft.EntityFrameworkCore;

namespace OpenAlprWebhookProcessor.Data
{
    public class ProcessorContext : DbContext
    {
        public ProcessorContext(DbContextOptions<ProcessorContext> options)
            : base(options)
        {
        }

        public DbSet<PlateGroup> PlateGroups { get; set; }

        public DbSet<Camera> Cameras { get; set; }

        public DbSet<Agent> Agents { get; set; }

        public DbSet<Ignore> Ignores { get; set; }

        public DbSet<Alert> Alerts { get; set; }

        public DbSet<WebhookForward> WebhookForwards { get; set; }

        public DbSet<Pushover> PushoverAlertClients { get; set; }

        public DbSet<Enricher> Enrichers { get; set; }
    }
}
