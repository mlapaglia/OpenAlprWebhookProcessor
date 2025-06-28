using AutoMapper;
using Hangfire;
using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using OpenAlprWebhookProcessor.Server.Alerts;
using OpenAlprWebhookProcessor.Server.Alerts.Pushover;
using OpenAlprWebhookProcessor.Server.Data;
using OpenAlprWebhookProcessor.Server.Hydration;
using OpenAlprWebhookProcessor.Server.LicensePlates.Enricher;
using OpenAlprWebhookProcessor.Server.LicensePlates.Enricher.LicensePlateData;
using OpenAlprWebhookProcessor.Server.ProcessorHub;
using OpenAlprWebhookProcessor.Server.SystemLogs;
using OpenAlprWebhookProcessor.Server.Users;
using OpenAlprWebhookProcessor.Server.Users.Data;
using OpenAlprWebhookProcessor.Server.Users.Register;
using OpenAlprWebhookProcessor.Server.WebhookProcessor;
using OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprAgentScraper;
using OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebsocket;
using OpenAlprWebhookProcessor.Server.WebPushSubscriptions;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server
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

            var processorOptionsBuilder = new DbContextOptionsBuilder<ProcessorContext>();
            processorOptionsBuilder.UseSqlite(ProcessorContextConnectionString);

            Directory.CreateDirectory(configurationDirectory);

            using (var context = new ProcessorContext(processorOptionsBuilder.Options))
            {
                context.Database.Migrate();
                var agent = context.Agents.FirstOrDefault();

                if (agent == null)
                {
                    agent = new Agent();

                    context.Agents.Add(agent);
                    context.SaveChanges();
                }
            }

            var optionsBuilder = new DbContextOptionsBuilder<UsersContext>();
            optionsBuilder.UseSqlite(UsersContextConnectionString);

            using (var context = new UsersContext(optionsBuilder.Options))
            {
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }

                var userService = new UserService(context);
                var secretKey = userService.GetJwtSecretKeyAsync(CancellationToken.None).Result;

                services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };
                    x.Events = new JwtBearerEvents
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
                });
            }

            services.AddScoped<IUserService, UserService>();

            services.AddDbContext<ProcessorContext>(options =>
                options.UseSqlite(ProcessorContextConnectionString));

            services.AddDbContext<UsersContext>(options =>
                options.UseSqlite(UsersContextConnectionString));

            var handlerTypes = Assembly.GetExecutingAssembly()
             .GetTypes()
             .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Handler"));

            foreach (var handlerType in handlerTypes)
            {
                services.TryAddScoped(handlerType);
            }

            services.AddScoped<OpenAlprAgentScraper>();

            services.AddScoped<ILicensePlateEnricherClient, LicensePlateDataClient>();

            services.AddSingleton<IAlertClient, PushoverClient>();
            services.AddSingleton<IAlertClient, WebPushNotificationProducer>();
            services.AddSingleton<IWebPushSubscriptionsService, WebPushSubscriptionsService>();

            services.AddHttpClient<PushServiceClient>();

            services.AddSingleton<WebPushNotificationProducer>();
            services.AddSingleton<IHostedService>(p => p.GetService<WebPushNotificationProducer>());

            services.AddSingleton<WebsocketClientOrganizer>();
            services.AddSingleton<IHostedService>(p => p.GetService<WebsocketClientOrganizer>());

            services.AddSingleton<CameraUpdateService.CameraUpdateService>();
            services.AddSingleton<IHostedService>(p => p.GetService<CameraUpdateService.CameraUpdateService>());

            services.AddSingleton<HydrationService>();
            services.AddSingleton<IHostedService>(p => p.GetService<HydrationService>());

            services.AddSingleton<AlertService>();
            services.AddSingleton<IHostedService>(p => p.GetService<AlertService>());

            services.AddSingleton<ImageRetrieverService>();
            services.AddSingleton<IHostedService>(p => p.GetService<ImageRetrieverService>());

            var mapper = new MapperConfiguration(mc =>
            {
                mc.CreateMap<User, UserModel>();
                mc.CreateMap<User, UserModel>();
                mc.CreateMap<RegisterModel, User>();
                mc.CreateMap<UpdateModel, User>();
            });

            services.AddSingleton(mapper.CreateMapper());

            services.AddHttpClient();

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
            app.UseSerilogRequestLogging();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseHangfireDashboard();

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

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
