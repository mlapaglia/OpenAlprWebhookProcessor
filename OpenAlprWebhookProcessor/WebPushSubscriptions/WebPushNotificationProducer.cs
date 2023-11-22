using Lib.Net.Http.WebPush.Authentication;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Hosting;
using System.Threading;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebPushSubscriptions.VapidKeys;
using System.Threading.Tasks;
using System;
using OpenAlprWebhookProcessor.Alerts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Alerts.Pushover;
using System.Collections.Generic;

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
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var keys = VapidKeyHelper.GetVapidKeys(processorContext);

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
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();
                var clientSettings = await processorContext.WebPushSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                if (clientSettings.IsEnabled && (alert.IsUrgent || clientSettings.SendEveryPlateEnabled))
                {
                    PushMessage notification = new AngularWebPushNotification
                    {
                        Body = $"Plate {alert.PlateNumber} seen at {DateTimeOffset.UtcNow:g}",
                        Icon = "assets/icons/icon-96x96.png",
                        Image = alert.PlateJpegUrl,
                        Title = $"Plate Seen: {alert.PlateNumber}",
                        Data = new Dictionary<string, object>()
                        {
                            { "plateid", alert.PlateId },
                            { "url", $"plate/{alert.PlateId}" }
                        }
                    }.ToPushMessage();

                    foreach (PushSubscription subscription in _pushSubscriptionsService.GetAll())
                    {
                        try
                        {
                            await _pushClient.RequestPushMessageDeliveryAsync(
                                subscription,
                                notification,
                                cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send WebPush message");
                            _pushSubscriptionsService.Delete(subscription.Endpoint);
                        }
                    }
                }
            }
        }

        public async Task VerifyCredentialsAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var keys = await VapidKeyHelper.GetVapidKeysAsync(
                    processorContext,
                    cancellationToken);

                var credentialsValid = !string.IsNullOrWhiteSpace(keys.PrivateKey)
                    && !string.IsNullOrWhiteSpace(keys.PublicKey)
                    && !string.IsNullOrWhiteSpace(keys.Subject);

                if (!credentialsValid)
                {
                    _logger.LogError("WebPush credentials are missing.");
                }
                else
                {
                    _logger.LogInformation("WebPush credentials are present.");
                }
            }
        }

        public async Task<bool> ShouldSendAllPlatesAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<PushoverClient>>();

                logger.LogInformation("Sending Alert via Pushover.");

                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var clientSettings = await processorContext.PushoverAlertClients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                return clientSettings.SendEveryPlateEnabled;
            }
        }
    }
}
