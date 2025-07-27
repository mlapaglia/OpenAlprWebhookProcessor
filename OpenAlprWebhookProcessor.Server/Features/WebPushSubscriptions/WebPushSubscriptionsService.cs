using Lib.Net.Http.WebPush;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions
{
    internal partial class WebPushSubscriptionsService : IWebPushSubscriptionsService
    {
        private readonly IServiceProvider _serviceProvider;

        public WebPushSubscriptionsService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<List<PushSubscription>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var subscriptions = await unitOfWork.WebPushSubscriptions.GetQueryable()
                    .Include(x => x.Keys)
                    .ToListAsync(cancellationToken);

                var pushSubscriptions = new List<PushSubscription>();

                foreach (var subscription in subscriptions.Where(x => x.Keys != null && x.Keys.Any()))
                {
                    var authKey = subscription.Keys.FirstOrDefault(x => x.Key == "auth");
                    var p256dhKey = subscription.Keys.FirstOrDefault(x => x.Key == "p256dh");
                    
                    if (authKey == null || p256dhKey == null)
                        continue;

                    var newPushSubscription = new PushSubscription()
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

        public async Task InsertAsync(
            PushSubscription subscription,
            CancellationToken cancellationToken = default)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var existingSubscription = await unitOfWork.WebPushSubscriptions.GetQueryable()
                    .Include(x => x.Keys)
                    .FirstOrDefaultAsync(x => x.Endpoint == subscription.Endpoint);

                if (existingSubscription == null)
                {
                    var pushSubscription = new WebPushSubscription
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

                    await unitOfWork.WebPushSubscriptions.AddAsync(pushSubscription, cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }

        public async Task DeleteAsync(
            string endpoint,
            CancellationToken cancellationToken = default)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var endpointToRemove = await unitOfWork.WebPushSubscriptions.FirstOrDefaultAsync(x => x.Endpoint == endpoint, cancellationToken);
                if (endpointToRemove != null)
                {
                    unitOfWork.WebPushSubscriptions.Delete(endpointToRemove);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}
