using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Infrastructure.Behaviors;
using OpenAlprWebhookProcessor.Hydrator;
using Lib.Net.Http.WebPush;
using System.Reflection;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate.LicensePlateData;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate;
using System;
using System.Threading.Tasks;
using OpenAlprWebhookProcessor.Features.ImageRelay.ImageCompression;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Register;
using OpenAlprWebhookProcessor.Features.Alerts;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using OpenAlprWebhookProcessor.Features.MachineLearning.Configuration;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services.Filesystem;
using System.IO.Abstractions;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprAgentScraper;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using OpenAlprWebhookProcessor.Features.WebPushSubscriptions;
using OpenAlprWebhookProcessor.Features.ImageRelay.SnapshotRelay;

namespace OpenAlprWebhookProcessor.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;
            });
            
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            return services;
        }

        public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            var processorConnectionString = configuration.GetConnectionString("ProcessorConnection");

            var usersConnectionString = configuration.GetConnectionString("UsersConnection");

            services.AddDbContext<ProcessorContext>(options =>
                options.UseSqlite(processorConnectionString, sqliteOptions =>
                {
                    sqliteOptions.CommandTimeout(30);
                }));

            services.AddDbContext<UsersContext>(options =>
                options.UseSqlite(usersConnectionString, sqliteOptions =>
                {
                    sqliteOptions.CommandTimeout(30);
                }));

            services.AddScoped<IRepository<PlateGroup>, Repository<PlateGroup>>();
            services.AddScoped<IPlateGroupRepository, PlateGroupRepository>();
            services.AddScoped<IRepository<PlateGroupRaw>, Repository<PlateGroupRaw>>();
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
            services.AddScoped<IRepository<MachineLearningConfigurationRepository>, Repository<MachineLearningConfigurationRepository>>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        public static IServiceCollection AddBackgroundServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<WebPushNotificationProducer>();
            services.AddSingleton<IHostedService>(p => p.GetService<WebPushNotificationProducer>());

            services.AddOptions<WebsocketClientOrganizerConfiguration>()
                .Bind(configuration.GetSection("WebSocketClientOrganizer"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddSingleton<IWebsocketClientOrganizer, WebsocketClientOrganizer>();
            services.AddHostedService<WebsocketClientOrganizerHostedService>();

            services.AddOptions<SnapshotRelayConfiguration>()
                .Bind(configuration.GetSection("SnapshotRelay"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<ISimpleCameraScheduler, SimpleCameraScheduler>();
            services.AddHostedService<SimpleCameraSchedulerHostedService>();

            services.AddSingleton<IHydrationService, HydrationService>();
            services.AddHostedService<HydrationHostedService>();

            services.AddSingleton<IAlertService, AlertService>();
            services.AddHostedService<AlertHostedService>();

            services.AddSingleton<IImageRetrieverService, ImageRetrieverService>();
            services.AddHostedService<ImageRetrieverHostedService>();

            return services;
        }

        public static IServiceCollection AddMachineLearningServices(this IServiceCollection services)
        {
            services.AddSingleton<ILicensePlateMlTrainingService, LicensePlateMlTrainingService>();
            services.AddHostedService<LicensePlateMlTrainingHostedService>();

            services.AddScoped<ILicensePlateFeatureExtractor, LicensePlateFeatureExtractor>();
            services.AddScoped<ILicensePlatePredictionService, LicensePlatePredictionService>();

            services.AddSingleton<IMachineLearningConfiguration, Features.MachineLearning.Configuration.MachineLearningConfiguration>();

            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton<IModelPersistenceService, ModelPersistenceService>();

            return services;
        }

        public static IServiceCollection AddExternalServices(this IServiceCollection services)
        {
            services.AddScoped<IGroupWebhookHandler, GroupWebhookHandler>();
            services.AddScoped<IWebhookForwarder, WebhookForwarder>();

            services.AddScoped<SinglePlateWebhookHandler>();
            services.AddScoped<IOpenAlprAgentScraper, OpenAlprAgentScraper>();
            services.AddScoped<IImageCompressionService, ImageCompressionService>();
            services.AddScoped<ITimeService, TimeService>();
            services.AddScoped<ILicensePlateEnricherClient, LicensePlateDataClient>();
            services.AddSingleton<IAlertClient, PushoverClient>();
            services.AddSingleton<IAlertClient, WebPushNotificationProducer>();
            services.AddSingleton<IWebPushSubscriptionsService, WebPushSubscriptionsService>();
            services.AddSingleton(provider => 
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();
                return new PushServiceClient(httpClient);
            });

            services.AddSingleton<IPushServiceClientWrapper, PushServiceClientWrapper>();
            services.AddSingleton<Features.Cameras.ICameraFactory, Features.Cameras.CameraFactory>();

            return services;
        }

        public static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.CreateMap<User, UserModel>();
                cfg.CreateMap<RegisterModel, User>();
                cfg.CreateMap<UpdateModel, User>();
            });

            return services;
        }

        public static IServiceCollection AddDevelopmentDataSeeding(this IServiceCollection services)
        {
            services.AddScoped<DevelopmentDataSeeder>();
            return services;
        }

        public static async Task SeedDevelopmentDataAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>();
            await seeder.SeedAsync();
        }
    }
} 