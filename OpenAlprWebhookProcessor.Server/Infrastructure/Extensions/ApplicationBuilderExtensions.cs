using Microsoft.AspNetCore.Builder;
using OpenAlprWebhookProcessor.Infrastructure.Middleware;

namespace OpenAlprWebhookProcessor.Infrastructure.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            return app;
        }
    }
} 