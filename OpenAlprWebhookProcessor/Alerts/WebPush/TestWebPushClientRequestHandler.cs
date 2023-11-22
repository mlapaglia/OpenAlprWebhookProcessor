using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebPushSubscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts.WebPush
{
    public class TestWebPushClientRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly IAlertClient _alertClient;

        public TestWebPushClientRequestHandler(ProcessorContext processorContext,
            IEnumerable<IAlertClient> alertClients)
        {
            _processorContext = processorContext;
            _alertClient = alertClients.First(x => x is WebPushNotificationProducer);
        }

        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            var testPlateGroup = await _processorContext.PlateGroups
                .Include(x => x.PlateImage)
                .Where(x => x.PlateImage != null)
                .FirstOrDefaultAsync(cancellationToken);

            await _alertClient.VerifyCredentialsAsync(cancellationToken);

            await _alertClient.SendAlertAsync(new AlertUpdateRequest()
            {
                Description = "was seen on " + DateTimeOffset.UtcNow.ToString("g"),
                PlateNumber = testPlateGroup.BestNumber,
                PlateJpeg = testPlateGroup.PlateImage.Jpeg,
                PlateJpegUrl = $"/images/crop/{testPlateGroup.OpenAlprUuid}",
                IsUrgent = true,
                ReceivedOn = DateTimeOffset.UtcNow,
            }, cancellationToken);
        }
    }
}
