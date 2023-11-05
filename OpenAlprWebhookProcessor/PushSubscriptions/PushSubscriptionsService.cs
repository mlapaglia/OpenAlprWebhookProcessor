using Lib.Net.Http.WebPush;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenAlprWebhookProcessor.PushSubscriptions
{
    internal partial class PushSubscriptionsService : IPushSubscriptionsService
    {
        private readonly IServiceProvider _serviceProvider;

        public PushSubscriptionsService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public List<PushSubscription> GetAll()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var subscriptions = processorContext.PushSubscriptions
                    .Include(x => x.Keys)
                    .AsNoTracking()
                    .ToList();

                var pushSubscriptions = new List<PushSubscription>();

                foreach (var subscription in subscriptions.Where(x => x.Keys != null))
                {
                    var newPushSubscription = new PushSubscription()
                    {
                        Endpoint = subscription.Endpoint,
                        Keys = new Dictionary<string, string>(),
                    };

                    foreach (var key in subscription.Keys)
                    {
                        newPushSubscription.Keys.Add(key.Key, key.Value);
                    }

                    pushSubscriptions.Add(newPushSubscription);
                }

                return pushSubscriptions;
            }  
        }

        public void Insert(PushSubscription subscription)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var existingSubscription = processorContext.PushSubscriptions.FirstOrDefault(x => x.Endpoint == subscription.Endpoint);

                if (existingSubscription == null)
                {
                    var pushSubscription = new MobilePushSubscription
                    {
                        Endpoint = subscription.Endpoint,
                        Keys = new List<MobilePushSubscriptionKey>(),
                    };

                    foreach (var key in subscription.Keys)
                    {
                        pushSubscription.Keys.Add(new MobilePushSubscriptionKey()
                        {
                            Key = key.Key,
                            Value = key.Value,
                        });
                    }

                    processorContext.PushSubscriptions.Add(pushSubscription);
                    processorContext.SaveChanges();
                }
            }
        }

        public void Delete(string endpoint)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var endpointToRemove = processorContext.PushSubscriptions.FirstOrDefault(x => x.Endpoint == endpoint);
                processorContext.PushSubscriptions.Remove(endpointToRemove);
                processorContext.SaveChanges();

            }
        }
    }
}
