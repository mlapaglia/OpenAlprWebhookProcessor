using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetMostSeenPlates;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetMostSeenPlates
{
    public class GetMostSeenPlatesQueryHandler : IRequestHandler<GetMostSeenPlatesQuery, GetMostSeenPlatesResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetMostSeenPlatesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetMostSeenPlatesResponse> Handle(GetMostSeenPlatesQuery request, CancellationToken cancellationToken)
        {
            var plateGroups = await _unitOfWork.PlateGroups.GetMostSeenPlatesAsync(
                request.StartDate,
                request.EndDate,
                request.Limit,
                cancellationToken);

            var platesToIgnore = await GetPlatesToIgnoreAsync(cancellationToken);
            var platesToAlert = await GetPlatesToAlertAsync(cancellationToken);

            var licensePlates = plateGroups.GroupBy(x => x.BestNumber)
                .Select(x => new MostSeenCount
                {
                    PlateNumber = x.Key,
                    Count = x.Count(),
                }).ToList();

            return new GetMostSeenPlatesResponse
            {
                Counts = licensePlates
            };
        }

        private async Task<List<string>> GetPlatesToIgnoreAsync(CancellationToken cancellationToken)
        {
            var ignores = await _unitOfWork.Ignores.GetAllAsync(cancellationToken);
            return ignores.Select(x => x.PlateNumber).ToList();
        }

        private async Task<List<string>> GetPlatesToAlertAsync(CancellationToken cancellationToken)
        {
            var alerts = await _unitOfWork.Alerts.GetAllAsync(cancellationToken);
            return alerts.Select(x => x.PlateNumber).ToList();
        }
    }
} 