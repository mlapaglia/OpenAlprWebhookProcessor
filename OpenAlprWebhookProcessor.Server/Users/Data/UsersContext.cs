using Microsoft.EntityFrameworkCore;

namespace OpenAlprWebhookProcessor.Users.Data
{
    public class UsersContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<JwtKey> JwtKeys { get; set; }

        public UsersContext(DbContextOptions<UsersContext> options)
            : base(options)
        {
        }
    }
}
