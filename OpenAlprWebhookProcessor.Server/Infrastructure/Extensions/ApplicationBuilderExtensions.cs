using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Infrastructure.Middleware;
using OpenAlprWebhookProcessor.Users.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Infrastructure.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            return app;
        }

        public static async Task<IApplicationBuilder> EnsureDatabasesCreatedAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            // Ensure directories exist
            Directory.CreateDirectory("config");

            // Migrate ProcessorContext
            var processorContext = services.GetRequiredService<ProcessorContext>();
            await processorContext.Database.MigrateAsync();

            // Ensure default agent exists
            var agent = await processorContext.Agents.FirstOrDefaultAsync();
            if (agent == null)
            {
                agent = new Data.Agent();
                processorContext.Agents.Add(agent);
                await processorContext.SaveChangesAsync();
            }

            // Migrate UsersContext
            var usersContext = services.GetRequiredService<UsersContext>();
            if ((await usersContext.Database.GetPendingMigrationsAsync()).Any())
            {
                await usersContext.Database.MigrateAsync();
            }

            return app;
        }
    }
} 