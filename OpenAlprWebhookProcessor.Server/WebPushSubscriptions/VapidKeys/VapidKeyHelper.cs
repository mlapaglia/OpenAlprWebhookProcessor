using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebPushSubscriptions.VapidKeys
{
    public static class VapidKeyHelper
    {
        public static async Task<VapidDetails> GetVapidKeysAsync(
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken = default)
        {
            var pushSettings = await unitOfWork.WebPushSettings.GetFirstAsync(cancellationToken);

            if (pushSettings == null || string.IsNullOrWhiteSpace(pushSettings.PublicKey))
            {
                pushSettings = await AddVapidKeysAsync(unitOfWork, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return new VapidDetails()
            {
                PublicKey = pushSettings.PublicKey,
                PrivateKey = pushSettings.PrivateKey,
            };
        }

        public static async Task<WebPushSettings> AddVapidKeysAsync(
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken = default)
        {
            var vapidKeys = VapidKeyGenerator.GenerateVapidKeys();

            var pushSettings = new WebPushSettings()
            {
                PublicKey = vapidKeys.PublicKey,
                PrivateKey = vapidKeys.PrivateKey,
            };

            await unitOfWork.WebPushSettings.AddAsync(
                pushSettings,
                cancellationToken);

            return pushSettings;
        }
    }
}
