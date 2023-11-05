using Lib.Net.Http.WebPush.Authentication;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Threading;

namespace OpenAlprWebhookProcessor.PushSubscriptions
{
    public class PushNotificationProducer : BackgroundService
    {
        private readonly IPushSubscriptionsService _pushSubscriptionsService;
        private readonly PushServiceClient _pushClient;

        public PushNotificationProducer(
            IOptions<PushNotificationsOptions> options,
            IPushSubscriptionsService pushSubscriptionsService,
            PushServiceClient pushClient)
        {
            _pushSubscriptionsService = pushSubscriptionsService;
            _pushClient = pushClient;
            _pushClient.DefaultAuthentication = new VapidAuthentication(options.Value.PublicKey, options.Value.PrivateKey)
            {
                Subject = "https://angular-aspnetmvc-pushnotifications.demo.io"
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
        }

        public void SendNotification(int temperatureC, CancellationToken stoppingToken)
        {
            PushMessage notification = new AngularPushNotification
            {
                Title = "New Weather Forecast",
                Body = $"Temp. (C): {temperatureC} | Temp. (F): {32 + (int)(temperatureC / 0.5556)}",
                Icon = "assets/icons/icon-96x96.png"
            }.ToPushMessage();

            foreach (PushSubscription subscription in _pushSubscriptionsService.GetAll())
            {
                _pushClient.RequestPushMessageDeliveryAsync(subscription, notification, stoppingToken);
            }
        }
    }
}
