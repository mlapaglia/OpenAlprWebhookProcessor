using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAlprWebhookProcessor.Alerts;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Infrastructure.Behaviors;
using OpenAlprWebhookProcessor.WebhookProcessor;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprAgentScraper;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket;
using OpenAlprWebhookProcessor.WebPushSubscriptions;
using OpenAlprWebhookProcessor.Alerts.Pushover;
using OpenAlprWebhookProcessor.Hydrator;
using Lib.Net.Http.WebPush;
using System.Reflection;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate.LicensePlateData;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenAlprWebhookProcessor.Features.ImageRelay.ImageCompression;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Register;

namespace OpenAlprWebhookProcessor.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            return services;
        }

        public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            var processorConnectionString = configuration.GetConnectionString("ProcessorConnection") 
                ?? "Data Source=config/processor.db;foreign keys=true;";
            
            var usersConnectionString = configuration.GetConnectionString("UsersConnection")
                ?? "Data Source=config/users.db";

            services.AddDbContext<ProcessorContext>(options =>
                options.UseSqlite(processorConnectionString));

            services.AddDbContext<UsersContext>(options =>
                options.UseSqlite(usersConnectionString));

            services.AddScoped<IRepository<PlateGroup>, Repository<PlateGroup>>();
            services.AddScoped<IPlateGroupRepository, PlateGroupRepository>();
            services.AddScoped<IAgentRepository, AgentRepository>();
            services.AddScoped<IRepository<Data.Alert>, Repository<Data.Alert>>();
            services.AddScoped<IRepository<Ignore>, Repository<Ignore>>();
            services.AddScoped<IRepository<Data.Camera>, Repository<Data.Camera>>();
            services.AddScoped<IRepository<CameraMask>, Repository<CameraMask>>();
            services.AddScoped<IRepository<Enricher>, Repository<Enricher>>();
            services.AddScoped<IRepository<WebhookForward>, Repository<WebhookForward>>();
            services.AddScoped<IRepository<Pushover>, Repository<Pushover>>();
            services.AddScoped<IRepository<WebPushSubscription>, Repository<WebPushSubscription>>();
            services.AddScoped<IRepository<WebPushSettings>, Repository<WebPushSettings>>();
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            services.AddSingleton<WebPushNotificationProducer>();
            services.AddSingleton<IHostedService>(p => p.GetService<WebPushNotificationProducer>());

            services.AddSingleton<WebsocketClientOrganizer>();
            services.AddSingleton<IWebsocketClientOrganizer>(p => p.GetService<WebsocketClientOrganizer>());
            services.AddSingleton<IHostedService>(p => p.GetService<WebsocketClientOrganizer>());

            services.AddSingleton<CameraUpdateService.CameraUpdateService>();
            services.AddSingleton<ICameraUpdateService>(p => p.GetService<CameraUpdateService.CameraUpdateService>());
            services.AddSingleton<IHostedService>(p => p.GetService<CameraUpdateService.CameraUpdateService>());
            
            services.AddScoped<ICameraScheduling, CameraUpdateService.CameraScheduling>();
            services.AddScoped<IBackgroundJobService, CameraUpdateService.BackgroundJobService>();

            services.AddSingleton<HydrationService>();
            services.AddSingleton<IHostedService>(p => p.GetService<HydrationService>());

            services.AddSingleton<AlertService>();
            services.AddSingleton<IAlertService>(p => p.GetService<AlertService>());
            services.AddSingleton<IHostedService>(p => p.GetService<AlertService>());

            services.AddSingleton<ImageRetrieverService>();
            services.AddSingleton<IHostedService>(p => p.GetService<ImageRetrieverService>());

            return services;
        }

        public static IServiceCollection AddExternalServices(this IServiceCollection services)
        {
            services.AddScoped<IGroupWebhookHandler, GroupWebhookHandler>();
            services.AddScoped<SinglePlateWebhookHandler>();
            services.AddScoped<IOpenAlprAgentScraper, OpenAlprAgentScraper>();
            services.AddScoped<IImageRetrieverService, ImageRetrieverService>();
            services.AddScoped<ITimeService, TimeService>();
            services.AddScoped<ILicensePlateEnricherClient, LicensePlateDataClient>();
            services.AddSingleton<IAlertClient, PushoverClient>();
            services.AddSingleton<IAlertClient, WebPushNotificationProducer>();
            services.AddSingleton<IWebPushSubscriptionsService, WebPushSubscriptionsService>();
            services.AddHttpClient<PushServiceClient>();
            services.AddHttpClient();
            services.AddScoped<IImageCompressionService, ImageCompressionService>();
            services.AddScoped<OpenAlprWebhookProcessor.Features.Cameras.ICameraFactory, OpenAlprWebhookProcessor.Features.Cameras.CameraFactory>();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
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

            // Configure JWT options using the service provider
            services.ConfigureOptions<JwtBearerPostConfigureOptions>();

            return services;
        }

        public static IServiceCollection AddAutoMapper(this IServiceCollection services)
        {
            var mapper = new AutoMapper.MapperConfiguration(mc =>
            {
                mc.CreateMap<User, UserModel>();
                mc.CreateMap<RegisterModel, User>();
                mc.CreateMap<UpdateModel, User>();
            });

            services.AddSingleton(mapper.CreateMapper());
            return services;
        }
    }

    public class JwtBearerPostConfigureOptions : Microsoft.Extensions.Options.IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly IServiceProvider _serviceProvider;

        public JwtBearerPostConfigureOptions(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void PostConfigure(string name, JwtBearerOptions options)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                {
                    // Get the signing key from the database
                    using var scope = _serviceProvider.CreateScope();
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    var key = userService.GetJwtSecretKeyAsync().Result;
                    return new[] { new SymmetricSecurityKey(key) };
                },
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        }
    }
} 