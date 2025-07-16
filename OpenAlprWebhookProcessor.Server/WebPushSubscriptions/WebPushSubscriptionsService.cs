using Lib.Net.Http.WebPush;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
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
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public List<Lib.Net.Http.WebPush.PushSubscription> GetAll()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var subscriptions = unitOfWork.WebPushSubscriptions.GetQueryable()
                    .Include(x => x.Keys)
                    .ToList();

                var pushSubscriptions = new List<Lib.Net.Http.WebPush.PushSubscription>();

                foreach (var subscription in subscriptions.Where(x => x.Keys != null && x.Keys.Any()))
                {
                    var authKey = subscription.Keys.FirstOrDefault(x => x.Key == "auth");
                    var p256dhKey = subscription.Keys.FirstOrDefault(x => x.Key == "p256dh");
                    
                    // Skip subscriptions that don't have the required keys
                    if (authKey == null || p256dhKey == null)
                        continue;

                    var newPushSubscription = new Lib.Net.Http.WebPush.PushSubscription()
                    {
                        Endpoint = subscription.Endpoint,
                        Keys = new Dictionary<string, string>(),
                    };

                    newPushSubscription.SetKey(PushEncryptionKeyName.Auth, authKey.Value);
                    newPushSubscription.SetKey(PushEncryptionKeyName.P256DH, p256dhKey.Value);

                    pushSubscriptions.Add(newPushSubscription);
                }

                return pushSubscriptions;
            }  
        }

        public void Insert(Lib.Net.Http.WebPush.PushSubscription subscription)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var existingSubscription = unitOfWork.WebPushSubscriptions.GetQueryable()
                    .Include(x => x.Keys)
                    .FirstOrDefault(x => x.Endpoint == subscription.Endpoint);

                if (existingSubscription == null)
                {
                    var pushSubscription = new Data.WebPushSubscription
                    {
                        Endpoint = subscription.Endpoint,
                        Keys = new List<WebPushSubscriptionKey>(),
                    };

                    if (subscription.Keys != null)
                    {
                        foreach (var key in subscription.Keys)
                        {
                            pushSubscription.Keys.Add(new WebPushSubscriptionKey()
                            {
                                Key = key.Key,
                                Value = key.Value,
                            });
                        }
                    }

                    unitOfWork.WebPushSubscriptions.AddAsync(pushSubscription).GetAwaiter().GetResult();
                    unitOfWork.SaveChangesAsync().GetAwaiter().GetResult();
                }
            }
        }

        public void Delete(string endpoint)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var endpointToRemove = unitOfWork.WebPushSubscriptions.FirstOrDefaultAsync(x => x.Endpoint == endpoint).GetAwaiter().GetResult();
                if (endpointToRemove != null)
                {
                    unitOfWork.WebPushSubscriptions.Delete(endpointToRemove);
                    unitOfWork.SaveChangesAsync().GetAwaiter().GetResult();
                }
            }
        }
    }
}
