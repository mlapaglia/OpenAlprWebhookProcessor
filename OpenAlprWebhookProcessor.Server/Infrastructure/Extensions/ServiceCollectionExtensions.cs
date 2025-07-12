using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAlprWebhookProcessor.Alerts;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Infrastructure.Behaviors;
using OpenAlprWebhookProcessor.LicensePlates.Enricher;
using OpenAlprWebhookProcessor.LicensePlates.Enricher.LicensePlateData;
using OpenAlprWebhookProcessor.Users.Data;
using OpenAlprWebhookProcessor.WebhookProcessor;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprAgentScraper;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket;
using OpenAlprWebhookProcessor.WebPushSubscriptions;
using OpenAlprWebhookProcessor.Alerts.Pushover;
using OpenAlprWebhookProcessor.Hydrator;
using Lib.Net.Http.WebPush;
using System.Reflection;

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
            services.AddScoped<IRepository<Camera>, Repository<Camera>>();
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
            services.AddSingleton<IHostedService>(p => p.GetService<WebsocketClientOrganizer>());

            services.AddSingleton<CameraUpdateService.CameraUpdateService>();
            services.AddSingleton<IHostedService>(p => p.GetService<CameraUpdateService.CameraUpdateService>());

            services.AddSingleton<HydrationService>();
            services.AddSingleton<IHostedService>(p => p.GetService<HydrationService>());

            services.AddSingleton<AlertService>();
            services.AddSingleton<IHostedService>(p => p.GetService<AlertService>());

            services.AddSingleton<ImageRetrieverService>();
            services.AddSingleton<IHostedService>(p => p.GetService<ImageRetrieverService>());

            return services;
        }

        public static IServiceCollection AddExternalServices(this IServiceCollection services)
        {
            services.AddScoped<OpenAlprAgentScraper>();
            services.AddScoped<ILicensePlateEnricherClient, LicensePlateDataClient>();
            services.AddSingleton<IAlertClient, PushoverClient>();
            services.AddSingleton<IAlertClient, WebPushNotificationProducer>();
            services.AddSingleton<IWebPushSubscriptionsService, WebPushSubscriptionsService>();
            services.AddHttpClient<PushServiceClient>();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Note: JWT key setup will be handled during startup
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                // Token validation parameters will be configured during startup
            });

            return services;
        }

        public static IServiceCollection AddAutoMapper(this IServiceCollection services)
        {
            var mapper = new AutoMapper.MapperConfiguration(mc =>
            {
                mc.CreateMap<Users.User, Users.UserModel>();
                mc.CreateMap<Users.Register.RegisterModel, Users.User>();
                mc.CreateMap<Users.UpdateModel, Users.User>();
            });

            services.AddSingleton(mapper.CreateMapper());
            return services;
        }
    }
} 