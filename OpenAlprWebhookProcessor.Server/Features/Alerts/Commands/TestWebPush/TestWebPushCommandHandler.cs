using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.WebPushSubscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.TestWebPush
{
    public class TestWebPushCommandHandler : IRequestHandler<TestWebPushCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAlertClient _alertClient;

        public TestWebPushCommandHandler(
            IUnitOfWork unitOfWork,
            IEnumerable<IAlertClient> alertClients)
        {
            _unitOfWork = unitOfWork;
            _alertClient = alertClients.First(x => x is WebPushNotificationProducer);
        }

        public async Task Handle(TestWebPushCommand request, CancellationToken cancellationToken)
        {
            var testPlateGroup = await _unitOfWork.PlateGroups.GetQueryable()
                .Include(x => x.PlateImage)
                .Where(x => x.PlateImage != null)
                .FirstOrDefaultAsync(cancellationToken);

            if (testPlateGroup == null)
            {
                throw new InvalidOperationException("No test plate group with image found in the database");
            }

            await _alertClient.VerifyCredentialsAsync(cancellationToken);

            await _alertClient.SendAlertAsync(new AlertUpdateRequest()
            {
                Description = "was seen on " + DateTimeOffset.UtcNow.ToString("g"),
                IsUrgent = true,
                PlateId = testPlateGroup.Id,
                PlateNumber = testPlateGroup.BestNumber,
                PlateJpeg = testPlateGroup.PlateImage.Jpeg,
                PlateJpegUrl = $"/api/images/crop/{testPlateGroup.OpenAlprUuid}",
                ReceivedOn = DateTimeOffset.UtcNow,
            }, cancellationToken);
        }
    }
} 