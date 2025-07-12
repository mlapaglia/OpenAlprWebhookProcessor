using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Server.Features.LicensePlates.Queries.GetLicensePlateCounts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts
{
    public class GetLicensePlateCountsQueryHandler : IRequestHandler<GetLicensePlateCountsQuery, GetLicensePlateCountsResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetLicensePlateCountsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetLicensePlateCountsResponse> Handle(GetLicensePlateCountsQuery request, CancellationToken cancellationToken)
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