using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Infrastructure.Behaviors;
using OpenAlprWebhookProcessor.WebhookProcessor;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprAgentScraper;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket;
using OpenAlprWebhookProcessor.WebPushSubscriptions;
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
using OpenAlprWebhookProcessor.Features.Alerts;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using OpenAlprWebhookProcessor.Features.MachineLearning.Configuration;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services.Filesystem;
using System.IO.Abstractions;

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
            var processorConnectionString = configuration.GetConnectionString("ProcessorConnection");

            var usersConnectionString = configuration.GetConnectionString("UsersConnection");

            services.AddDbContext<ProcessorContext>(options =>
                options.UseSqlite(processorConnectionString));

            services.AddDbContext<UsersContext>(options =>
                options.UseSqlite(usersConnectionString));

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

        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            services.AddSingleton<WebPushNotificationProducer>();
            services.AddSingleton<IHostedService>(p => p.GetService<WebPushNotificationProducer>());

            services.AddSingleton<IWebsocketClientOrganizer, WebsocketClientOrganizer>();
            services.AddHostedService<WebsocketClientOrganizerHostedService>();

            services.AddSingleton<ICameraUpdateService, CameraUpdateService.CameraUpdateService>();
            services.AddHostedService<CameraUpdateHostedService>();

            services.AddSingleton<IBackgroundJobService, TimerBasedBackgroundJobService>();

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
            
            // Register IFileSystem for testable file operations
            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton<IModelPersistenceService, ModelPersistenceService>();

            return services;
        }

        public static IServiceCollection AddExternalServices(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddScoped<IGroupWebhookHandler, GroupWebhookHandler>();
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

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrWhiteSpace(context.Token))
                        {
                            // Handle SignalR connections via query string
                            var accessToken = context.HttpContext.Request.Query["access_token"];
                            if (!string.IsNullOrEmpty(accessToken) && 
                                context.HttpContext.Request.Path.StartsWithSegments("/api/processorHub", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = accessToken;
                            }
                            // Handle image requests via cookies
                            else if (context.HttpContext.Request.Path.StartsWithSegments("/api/images", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = context.Request.Cookies["jwtToken"];
                            }
                        }

                        return Task.CompletedTask;
                    }
                };
            });

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