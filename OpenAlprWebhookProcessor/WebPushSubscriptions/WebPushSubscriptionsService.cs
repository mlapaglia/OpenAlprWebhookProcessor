using Lib.Net.Http.WebPush;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenAlprWebhookProcessor.WebPushSubscriptions
{
    internal partial class WebPushSubscriptionsService : IWebPushSubscriptionsService
    {
        private readonly IServiceProvider _serviceProvider;

        public WebPushSubscriptionsService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public List<Lib.Net.Http.WebPush.PushSubscription> GetAll()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var subscriptions = processorContext.WebPushSubscriptions
                    .Include(x => x.Keys)
                    .AsNoTracking()
                    .ToList();

                var pushSubscriptions = new List<Lib.Net.Http.WebPush.PushSubscription>();

                foreach (var subscription in subscriptions.Where(x => x.Keys != null))
                {
                    var newPushSubscription = new Lib.Net.Http.WebPush.PushSubscription()
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

        public void Insert(Lib.Net.Http.WebPush.PushSubscription subscription)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var existingSubscription = processorContext.WebPushSubscriptions
                    .Include(x => x.Keys)
                    .FirstOrDefault(x => x.Endpoint == subscription.Endpoint);

                if (existingSubscription == null)
                {
                    var pushSubscription = new Data.WebPushSubscription
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
                    processorContext.SaveChanges();
                }
            }
        }

        public void Delete(string endpoint)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var endpointToRemove = processorContext.WebPushSubscriptions.FirstOrDefault(x => x.Endpoint == endpoint);
                processorContext.WebPushSubscriptions.Remove(endpointToRemove);
                processorContext.SaveChanges();

            }
        }
    }
}
