using Lib.Net.Http.WebPush;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.WebPushSubscriptions
{
    internal partial class WebPushSubscriptionsService : IWebPushSubscriptionsService
    {
        private readonly IServiceProvider _serviceProvider;

        public WebPushSubscriptionsService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<List<PushSubscription>> GetAllAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var subscriptions = await processorContext.WebPushSubscriptions
                    .Include(x => x.Keys)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var pushSubscriptions = new List<PushSubscription>();

                foreach (var subscription in subscriptions.Where(x => x.Keys != null))
                {
                    var newPushSubscription = new PushSubscription()
                    {
                        Endpoint = subscription.Endpoint,
                        Keys = new Dictionary<string, string>(),
                    };

                    newPushSubscription.SetKey(PushEncryptionKeyName.Auth, subscription.Keys.First(x => x.Key == "auth").Value);
                    newPushSubscription.SetKey(PushEncryptionKeyName.P256DH, subscription.Keys.First(x => x.Key == "p256dh").Value);

                    pushSubscriptions.Add(newPushSubscription);
                }

                return pushSubscriptions;
            }  
        }

        public async Task InsertAsync(
            PushSubscription subscription,
            CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var existingSubscription = processorContext.WebPushSubscriptions
                    .Include(x => x.Keys)
                    .FirstOrDefaultAsync(x =>
                        x.Endpoint == subscription.Endpoint,
                        cancellationToken);

                if (existingSubscription == null)
                {
                    var pushSubscription = new WebPushSubscription
                    {
                        Endpoint = subscription.Endpoint,
                        Keys = new List<WebPushSubscriptionKey>(),
                    };

                    foreach (var key in subscription.Keys)
                    {
                        pushSubscription.Keys.Add(new WebPushSubscriptionKey()
                        {
                            Key = key.Key,
                            Value = key.Value,
                        });
                    }

                    processorContext.WebPushSubscriptions.Add(pushSubscription);
                    await processorContext.SaveChangesAsync(cancellationToken);
                }
            }
        }

        public async Task DeleteAsync(string endpoint, CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var endpointToRemove = await processorContext.WebPushSubscriptions.FirstOrDefaultAsync(x => x.Endpoint == endpoint, cancellationToken);
                processorContext.WebPushSubscriptions.Remove(endpointToRemove);
                await processorContext.SaveChangesAsync(cancellationToken);

            }
        }
    }
}
