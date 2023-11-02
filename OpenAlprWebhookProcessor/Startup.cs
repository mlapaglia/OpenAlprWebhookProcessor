using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenAlprWebhookProcessor.Alerts;
using OpenAlprWebhookProcessor.Cameras;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Hydrator;
using OpenAlprWebhookProcessor.ImageRelay;
using OpenAlprWebhookProcessor.LicensePlates.DeletePlate;
using OpenAlprWebhookProcessor.LicensePlates.GetLicensePlateCounts;
using OpenAlprWebhookProcessor.LicensePlates.SearchLicensePlates;
using OpenAlprWebhookProcessor.SystemLogs;
using OpenAlprWebhookProcessor.ProcessorHub;
using OpenAlprWebhookProcessor.Settings;
using OpenAlprWebhookProcessor.Settings.GetIgnores;
using OpenAlprWebhookProcessor.Settings.UpdatedCameras;
using OpenAlprWebhookProcessor.Settings.UpsertWebhookForwards;
using OpenAlprWebhookProcessor.Users;
using OpenAlprWebhookProcessor.Users.Data;
using OpenAlprWebhookProcessor.Users.Register;
using OpenAlprWebhookProcessor.WebhookProcessor;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using OpenAlprWebhookProcessor.Cameras.ZoomAndFocus;
using System.IO;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprAgentScraper;
using OpenAlprWebhookProcessor.LicensePlates.GetPlateFilters;
using OpenAlprWebhookProcessor.Settings.AgentHydration;
using OpenAlprWebhookProcessor.LicensePlates.GetStatistics;
using OpenAlprWebhookProcessor.LicensePlates.UpsertPlate;
using OpenAlprWebhookProcessor.Alerts.Pushover;
using OpenAlprWebhookProcessor.Settings.Enrichers;
using OpenAlprWebhookProcessor.LicensePlates.Enricher;
using OpenAlprWebhookProcessor.LicensePlates.Enricher.LicensePlateData;
using System.Text.Json.Serialization;
using OpenAlprWebhookProcessor.Settings.GetDebugPlateGroups;
using OpenAlprWebhookProcessor.Settings.GetDebubPlateGroups;
using OpenAlprWebhookProcessor.ImageRelay.GetImage;

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
            services
                .AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddSignalR();

            var processorOptionsBuilder = new DbContextOptionsBuilder<ProcessorContext>();
            processorOptionsBuilder.UseSqlite(ProcessorContextConnectionString);

            Directory.CreateDirectory(configurationDirectory);

            using (var context = new ProcessorContext(processorOptionsBuilder.Options))
            {
                Log.Logger.Information("Datbase migration started, this may take a while...");
                context.Database.Migrate();
                Log.Logger.Information("Database migration completed.");
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
                var secretKey = userService.GetJwtSecretKeyAsync().Result;

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
                                && context.HttpContext.Request.Path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = context.Request.Cookies["jwtToken"];
                            }
                            
                            return Task.CompletedTask;
                        }
                    };
                });
            }

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "angularapp/dist";
            });

            services.AddScoped<IUserService, UserService>();

            services.AddLogging(config =>
            {
                config.AddDebug();
                config.AddConsole();
            });

            services.AddDbContext<ProcessorContext>(options =>
                options.UseSqlite(ProcessorContextConnectionString));

            services.AddDbContext<UsersContext>(options =>
                options.UseSqlite(UsersContextConnectionString));

            services.AddScoped<GroupWebhookHandler>();
            services.AddScoped<SinglePlateWebhookHandler>();
            services.AddScoped<GetAgentRequestHandler>();
            services.AddScoped<GetCameraRequestHandler>();
            services.AddScoped<SetZoomAndFocusHandler>();
            services.AddScoped<GetZoomAndFocusHandler>();
            services.AddScoped<DeleteCameraHandler>();
            services.AddScoped<UpsertIgnoresRequestHandler>();
            services.AddScoped<TestCameraHandler>();
            services.AddScoped<UpsertAgentRequestHandler>();
            services.AddScoped<GetAlertsRequestHandler>();
            services.AddScoped<GetIgnoresRequestHandler>();
            services.AddScoped<UpsertCameraHandler>();
            services.AddScoped<SearchLicensePlateHandler>();
            services.AddScoped<UpsertAlertsRequestHandler>();
            services.AddScoped<GetSnapshotHandler>();
            services.AddScoped<GetLicensePlateCountsHandler>();
            services.AddScoped<DeleteLicensePlateGroupRequestHandler>();
            services.AddScoped<GetWebhookForwardsRequestHandler>();
            services.AddScoped<UpsertWebhookForwardsRequestHandler>();
            services.AddScoped<OpenAlprAgentScraper>();
            services.AddScoped<GetLicensePlateFiltersHandler>();
            services.AddScoped<AgentScrapeRequestHandler>();
            services.AddScoped<GetStatisticsHandler>();
            services.AddScoped<UpsertPlateRequestHandler>();
            services.AddScoped<UpsertPushoverClientRequestHandler>();
            services.AddScoped<GetPushoverClientRequestHandler>();
            services.AddScoped<TestPushoverClientRequestHandler>();
            services.AddScoped<GetEnrichersRequestHandler>();
            services.AddScoped<UpsertEnricherRequestHandler>();
            services.AddScoped<TestEnricherRequestHandler>();
            services.AddScoped<EnrichLicensePlateRequestHandler>();
            services.AddScoped<GetDebugPlateGroupRequestHandler>();
            services.AddScoped<DeleteDebugPlateGroupRequestHandler>();

            services.AddScoped<ILicensePlateEnricherClient, LicensePlateDataClient>();

            services.AddSingleton<IAlertClient, PushoverClient>();

            services.AddSingleton<CameraUpdateService.CameraUpdateService>();
            services.AddSingleton<IHostedService>(p => p.GetService<CameraUpdateService.CameraUpdateService>());

            services.AddSingleton<HydrationService>();
            services.AddSingleton<IHostedService>(p => p.GetService<HydrationService>());

            services.AddSingleton<AlertService>();
            services.AddSingleton<IHostedService>(p => p.GetService<AlertService>());

            services.AddSingleton<ImageRetrieverService>();
            services.AddSingleton<IHostedService>(p => p.GetService<ImageRetrieverService>());
            

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            var mapper = new MapperConfiguration(mc =>
            {
                mc.CreateMap<User, UserModel>();
                mc.CreateMap<User, UserModel>();
                mc.CreateMap<RegisterModel, User>();
                mc.CreateMap<UpdateModel, User>();
            });

            services.AddSingleton(mapper.CreateMapper());

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

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                app.UseHsts();
            }

            app.UseSerilogRequestLogging();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenALPR Webhook Processor");
                c.RoutePrefix = "/swagger";
            });

            app.UseStaticFiles();

            app.UseHangfireDashboard();

            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseMiddleware<JwtMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ProcessorHub.ProcessorHub>("/processorhub");
                endpoints.MapHangfireDashboard();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "angularapp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Debug)
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
