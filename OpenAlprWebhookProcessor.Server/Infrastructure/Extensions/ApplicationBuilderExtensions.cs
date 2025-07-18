using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Infrastructure.Middleware;
using System.IO;
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

            Directory.CreateDirectory("config");

            var processorContext = services.GetRequiredService<ProcessorContext>();
            await processorContext.Database.MigrateAsync();

            var agent = await processorContext.Agents.FirstOrDefaultAsync();
            if (agent == null)
            {
                agent = new Data.Agent();
                processorContext.Agents.Add(agent);
                await processorContext.SaveChangesAsync();
            }

            var usersContext = services.GetRequiredService<UsersContext>();
            await usersContext.Database.MigrateAsync();

            return app;
        }
    }
} 