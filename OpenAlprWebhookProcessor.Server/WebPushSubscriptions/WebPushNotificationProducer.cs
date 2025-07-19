using Lib.Net.Http.WebPush.Authentication;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Hosting;
using System.Threading;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.WebPushSubscriptions.VapidKeys;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using OpenAlprWebhookProcessor.Features.Alerts;

namespace OpenAlprWebhookProcessor.WebPushSubscriptions
{
    public class WebPushNotificationProducer : BackgroundService, IAlertClient
    {
        private readonly IWebPushSubscriptionsService _pushSubscriptionsService;

        private readonly PushServiceClient _pushClient;

        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<WebPushNotificationProducer> _logger;

        public WebPushNotificationProducer(
            IWebPushSubscriptionsService pushSubscriptionsService,
            IServiceProvider serviceProvider,
            ILogger<WebPushNotificationProducer> logger,
            PushServiceClient pushClient)
        {
            _pushSubscriptionsService = pushSubscriptionsService;
            _pushClient = pushClient;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var keys = await VapidKeyHelper.GetVapidKeysAsync(
                    unitOfWork, stoppingToken);

                _pushClient.DefaultAuthentication = new VapidAuthentication(
                    keys.PublicKey,
                    keys.PrivateKey)
                {
                    Subject = "mailto:" + keys.Subject,
                };
            }
        }

        public async Task SendAlertAsync(
            AlertUpdateRequest alert,
            CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var clientSettings = await unitOfWork.WebPushSettings.GetFirstAsync(cancellationToken);

                if (clientSettings != null && clientSettings.IsEnabled && (alert.IsUrgent || clientSettings.SendEveryPlateEnabled))
                {
                    PushMessage notification = new AngularWebPushNotification
                    {
                        Body = $"Plate {alert.PlateNumber} seen at {DateTimeOffset.UtcNow:g}",
                        Icon = "assets/icons/icon-96x96.png",
                        Image = alert.PlateJpegUrl,
                        Title = $"Plate Seen: {alert.PlateNumber}",
                        Data = new Dictionary<string, object>()
                        {
                            { "onActionClick", new Dictionary<string, object>()
                                {
                                    { "default", new Dictionary<string, object>()
                                        {
                                            { "operation", "navigateLastFocusedOrOpen" },
                                            {  "url", $"plate/{alert.PlateId}" }
                                        }
                                    }
                                }
                            }
                        }
                    }.ToPushMessage();

                    var subscriptions = await _pushSubscriptionsService.GetAllAsync(cancellationToken);
                    foreach (PushSubscription subscription in subscriptions)
                    {
                        try
                        {
                            await _pushClient.RequestPushMessageDeliveryAsync(subscription, notification, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send push notification.");
                        }
                    }
                }
            }
        }

        public async Task<bool> ShouldSendAllPlatesAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var clientSettings = await unitOfWork.WebPushSettings.GetFirstAsync(cancellationToken);

                return clientSettings?.SendEveryPlateEnabled ?? false;
            }
        }

        public async Task VerifyCredentialsAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var clientSettings = await unitOfWork.WebPushSettings.GetFirstAsync(cancellationToken);

                if (clientSettings == null)
                {
                    _logger.LogWarning("No WebPush settings found for verification.");
                    return;
                }

                _logger.LogInformation("WebPush credentials verified successfully.");
            }
        }
    }
}
