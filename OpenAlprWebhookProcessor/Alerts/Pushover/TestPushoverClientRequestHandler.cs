﻿using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
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
            IEnumerable<IAlertClient> alertClients)
        {
            _processorContext = processorContext;
            _alertClient = alertClients.First(x => x is PushoverClient);
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
                IsUrgent = false,
                ReceivedOn = DateTimeOffset.UtcNow,
            }, cancellationToken);
        }
    }
}
