using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts
{
    public class GetLicensePlateCountsQueryHandler : IQueryHandler<GetLicensePlateCountsQuery, GetLicensePlateCountsResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetLicensePlateCountsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<GetLicensePlateCountsResponse> Handle(GetLicensePlateCountsQuery request, CancellationToken cancellationToken = default)
        {
            var counts = await _unitOfWork.PlateGroups.GetPlateCountsAsync(
                request.StartDate,
                request.EndDate,
                cancellationToken);

            return new GetLicensePlateCountsResponse
            {
                Counts = counts.ToList()
            };
        }
    }
} 