using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts.Pushover
{
    public class TestPushoverClientRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly IAlertClient _alertClient;

        public TestPushoverClientRequestHandler(ProcessorContext processorContext,
            IAlertClient alertClient)
        {
            _processorContext = processorContext;
            _alertClient = alertClient;
        }

        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            var testPlateGroup = await _processorContext.PlateGroups
                .Include(x => x.PlateImage)
                .Where(x => x.PlateImage != null)
                .FirstOrDefaultAsync(cancellationToken);

            await _alertClient.VerifyCredentialsAsync(cancellationToken);

            await _alertClient.SendAlertAsync(new Alert()
            {
                Description = "was seen on " + DateTimeOffset.Now.ToString("g"),
                Id = testPlateGroup.Id,
                PlateNumber = testPlateGroup.BestNumber,
                StrictMatch = false,
            },
            testPlateGroup.PlateImage.Jpeg,
            cancellationToken);
        }
    }
}
