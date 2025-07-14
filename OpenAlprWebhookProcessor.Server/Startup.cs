using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenAlprWebhookProcessor.Infrastructure.Extensions;
using OpenAlprWebhookProcessor.ProcessorHub;
using OpenAlprWebhookProcessor.SystemLogs;
using OpenAlprWebhookProcessor.Users;
using OpenAlprWebhookProcessor.Users.Data;
using Serilog;
using System;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor
{
    public class Startup
    {
        private const string configurationDirectory = "config";

        private readonly string UsersContextConnectionString = $"Data Source={configurationDirectory}/users.db";

        private readonly string ProcessorContextConnectionString = $"Data Source={configurationDirectory}/processor.db;foreign keys=true;";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddControllers();
            services.AddSignalR();

            services.AddApplicationServices(Configuration);

            services.AddDataServices(Configuration);

            services.AddJwtAuthentication(Configuration);

            services.AddScoped<IUserService, UserService>();

            services.AddExternalServices();

            services.AddBackgroundServices();

            services.AddAutoMapper();

            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseInMemoryStorage());

            services.AddHangfireServer(options =>
            {
                options.SchedulePollingInterval = TimeSpan.FromSeconds(1);
            });

            services.AddMemoryCache();
        }

        public void Configure(IApplicationBuilder app)
        {
            // Configure JWT authentication with actual key
            ConfigureJwtAuthentication(app);

            app.EnsureDatabasesCreatedAsync().Wait();

            app.UseSerilogRequestLogging();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseHangfireDashboard();

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseExceptionHandling();

            app.UseMiddleware<JwtMiddleware>();

            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2)
            };

            app.UseWebSockets(webSocketOptions);

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("/index.html");
                endpoints.MapHub<ProcessorHub.ProcessorHub>("/api/processorHub");
            });

            ConfigureLogging(app);
        }

        private static void ConfigureJwtAuthentication(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var usersContext = scope.ServiceProvider.GetRequiredService<UsersContext>();
            var userService = new UserService(usersContext);
            var secretKey = userService.GetJwtSecretKeyAsync().Result;

            var jwtOptions = app.ApplicationServices.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<JwtBearerOptions>>();
            jwtOptions.CurrentValue.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
            jwtOptions.CurrentValue.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (string.IsNullOrWhiteSpace(context.Token)
                        && context.HttpContext.Request.Path.StartsWithSegments("/api/images", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Token = context.Request.Cookies["jwtToken"];
                    }

                    return Task.CompletedTask;
                }
            };
        }

        private static void ConfigureLogging(IApplicationBuilder app)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    "./config/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(5),
                    retainedFileCountLimit: 3)
                .WriteTo.Console()
                .WriteTo.Signalr(app.ApplicationServices.GetService<IHubContext<ProcessorHub.ProcessorHub, IProcessorHub>>())
                .CreateLogger();
        }
    }
}
