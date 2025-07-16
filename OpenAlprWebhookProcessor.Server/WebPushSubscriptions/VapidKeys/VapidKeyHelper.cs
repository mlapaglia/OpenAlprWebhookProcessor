using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebPushSubscriptions.VapidKeys
{
    public static class VapidKeyHelper
    {
        public static VapidDetails GetVapidKeys(ProcessorContext processorContext)
        {
            var pushSettings = processorContext.WebPushSettings.FirstOrDefault();

            if (pushSettings == null || string.IsNullOrWhiteSpace(pushSettings.PublicKey))
            {
                pushSettings = AddVapidKeys(processorContext);
                processorContext.SaveChanges();
            }

            return new VapidDetails()
            {
                Subject = pushSettings.Subject,
                PublicKey = pushSettings.PublicKey,
                PrivateKey = pushSettings.PrivateKey,
            };
        }

        public static async Task<VapidDetails> GetVapidKeysAsync(
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken)
        {
            var pushSettingsList = await unitOfWork.WebPushSettings.GetAllAsync(cancellationToken);
            var pushSettings = pushSettingsList.FirstOrDefault();

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

        public static WebPushSettings AddVapidKeys(ProcessorContext processorContext)
        {
            var vapidKeys = VapidKeyGenerator.GenerateVapidKeys();

            var pushSettings = new WebPushSettings()
            {
                PublicKey = vapidKeys.PublicKey,
                PrivateKey = vapidKeys.PrivateKey,
            };

            processorContext.WebPushSettings.Add(pushSettings);

            return pushSettings;
        }

        public static async Task<WebPushSettings> AddVapidKeysAsync(
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken)
        {
            var vapidKeys = VapidKeyGenerator.GenerateVapidKeys();

            var pushSettings = new WebPushSettings()
            {
                PublicKey = vapidKeys.PublicKey,
                PrivateKey = vapidKeys.PrivateKey,
            };

            await unitOfWork.WebPushSettings.AddAsync(pushSettings, cancellationToken);

            return pushSettings;
        }
    }
}
