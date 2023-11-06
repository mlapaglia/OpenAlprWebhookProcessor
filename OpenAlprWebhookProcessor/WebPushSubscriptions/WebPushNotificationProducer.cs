﻿using Lib.Net.Http.WebPush.Authentication;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Hosting;
using System.Threading;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebPushSubscriptions.VapidKeys;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using OpenAlprWebhookProcessor.Alerts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Flurl.Util;

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

            using(var scope = _serviceProvider.CreateScope())
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
            Alerts.Alert alert,
            byte[] plateJpeg,
            CancellationToken cancellationToken)
        {
            PushMessage notification = new AngularWebPushNotification
            {
                Title = $"Plate Seen: {alert.PlateNumber}",
                Body = $"Plate {alert.PlateNumber} seen at {DateTimeOffset.UtcNow:g}",
                Icon = "assets/icons/icon-96x96.png",
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
    }
}
