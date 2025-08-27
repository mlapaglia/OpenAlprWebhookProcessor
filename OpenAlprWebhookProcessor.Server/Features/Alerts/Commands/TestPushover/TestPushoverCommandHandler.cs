using Mediator;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.TestPushover
{
    public class TestPushoverCommandHandler : ICommandHandler<TestPushoverCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAlertClient _alertClient;

        public TestPushoverCommandHandler(
            IUnitOfWork unitOfWork, IEnumerable<IAlertClient> alertClients)
        {
            _unitOfWork = unitOfWork;
            _alertClient = alertClients.First(x => x is PushoverClient);
        }

        public async ValueTask<Unit> Handle(TestPushoverCommand command, CancellationToken cancellationToken)
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
                PlateJpeg = testPlateGroup.PlateImage.Jpeg,
                PlateJpegUrl = $"/api/images/crop/{testPlateGroup.OpenAlprUuid}",
                PlateNumber = testPlateGroup.BestNumber,
                ReceivedOn = DateTimeOffset.UtcNow,
            }, cancellationToken);

            return Unit.Value;
        }
    }
}
    