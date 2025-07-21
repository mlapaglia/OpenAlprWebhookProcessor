using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.Users.Data;
using Serilog;
using System;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Migrations", Serilog.Events.LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    "./config/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(5),
                    retainedFileCountLimit: 3)
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");

                var host = CreateHostBuilder(args).Build();

                using (var scope = host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var environment = services.GetRequiredService<IWebHostEnvironment>();

                    if (environment.IsDevelopment())
                    {
                        try
                        {
                            Log.Information("Running development migrations...");

                            var processorContext = services.GetRequiredService<ProcessorContext>();
                            await processorContext.Database.MigrateAsync();
                            Log.Information("ProcessorConnection migrations completed.");

                            var usersContext = services.GetRequiredService<UsersContext>();
                            await usersContext.Database.MigrateAsync();
                            Log.Information("UsersConnection migrations completed.");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Migration failed during development startup");
                            throw;
                        }
                    }
                }

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}