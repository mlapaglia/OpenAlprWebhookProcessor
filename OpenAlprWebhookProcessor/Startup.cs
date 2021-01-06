using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Cameras.Configuration;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.LicensePlates.GetLicensePlate;
using OpenAlprWebhookProcessor.WebhookProcessor;
using Serilog;
using System;
using System.Linq;

namespace OpenAlprWebhookProcessor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var agentConfiguration = new AgentConfiguration();
            Configuration.GetSection("OpenAlprAgent").Bind(agentConfiguration);

            if (agentConfiguration.Cameras == null || agentConfiguration.Cameras.Count == 0)
            {
                throw new ArgumentException("no cameras found in appsettings, check your configuration");
            }

            if (agentConfiguration.OpenAlprWebServer != null
                && agentConfiguration.OpenAlprWebServer.Endpoint != null)
            {
                services.AddHostedService<HeartbeatService.HeartbeatService>();
            }

            services.AddLogging(config =>
            {
                config.AddDebug();
                config.AddConsole();
            });

            services
                .AddEntityFrameworkSqlite()
                .AddDbContext<ProcessorContext>(options => options.UseSqlite(Configuration.GetConnectionString("ProcessorContext")));

            services.AddSingleton(agentConfiguration);

            services.AddScoped<WebhookHandler>();
            services.AddScoped<GetLicensePlateHandler>();

            services.AddSingleton<CameraUpdateService.CameraUpdateService>();
            services.AddSingleton<IHostedService>(p => p.GetService<CameraUpdateService.CameraUpdateService>());

            services.AddSwaggerGen();
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            MigrateDb(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenALPR Webhook Processor");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void MigrateDb(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                if (processorContext.Database.GetPendingMigrations().Any())
                {
                    processorContext.Database.Migrate();
                }
            }
        }
    }
}
