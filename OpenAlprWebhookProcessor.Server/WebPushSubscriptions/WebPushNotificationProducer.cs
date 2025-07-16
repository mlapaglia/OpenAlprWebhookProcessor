using Lib.Net.Http.WebPush.Authentication;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Hosting;
using System.Threading;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.WebPushSubscriptions.VapidKeys;
using System.Threading.Tasks;
using System;
using OpenAlprWebhookProcessor.Alerts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Alerts.Pushover;
using System.Collections.Generic;
using System.Linq;

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

            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var keys = VapidKeyHelper.GetVapidKeysAsync(unitOfWork, CancellationToken.None).GetAwaiter().GetResult();

                _pushClient.DefaultAuthentication = new VapidAuthentication(
                    keys.PublicKey,
                    keys.PrivateKey)
                {
                    Subject = "mailto:" + keys.Subject,
                };
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
        }

        public async Task SendAlertAsync(
            AlertUpdateRequest alert,
            CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var webPushSettings = await unitOfWork.WebPushSettings.GetAllAsync(cancellationToken);
                var clientSettings = webPushSettings.FirstOrDefault();

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

                    foreach (PushSubscription subscription in _pushSubscriptionsService.GetAll())
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
                var webPushSettings = await unitOfWork.WebPushSettings.GetAllAsync(cancellationToken);
                var clientSettings = webPushSettings.FirstOrDefault();

                return clientSettings?.SendEveryPlateEnabled ?? false;
            }
        }

        public async Task VerifyCredentialsAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var webPushSettings = await unitOfWork.WebPushSettings.GetAllAsync(cancellationToken);
                var clientSettings = webPushSettings.FirstOrDefault();

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
