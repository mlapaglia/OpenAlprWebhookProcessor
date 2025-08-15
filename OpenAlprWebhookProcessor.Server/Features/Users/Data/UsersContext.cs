using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Data
{
    public class UsersContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public UsersContext(DbContextOptions<UsersContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
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
    }
}
