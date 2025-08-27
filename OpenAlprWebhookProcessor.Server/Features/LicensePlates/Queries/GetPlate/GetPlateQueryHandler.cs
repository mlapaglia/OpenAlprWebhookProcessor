using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlate
{
    public class GetPlateQueryHandler : IQueryHandler<GetPlateQuery, LicensePlate?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPlateQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<LicensePlate?> Handle(GetPlateQuery request, CancellationToken cancellationToken)
        {
            var plateGroup = await _unitOfWork.PlateGroups.GetByIdWithDetailsAsync(request.Id, cancellationToken);
            
            if (plateGroup == null)
            {
                return null;
            }

            var platesToIgnore = await GetPlatesToIgnoreAsync(cancellationToken);
            var platesToAlert = await GetPlatesToAlertAsync(cancellationToken);

            return PlateMapper.MapPlate(plateGroup, platesToIgnore, platesToAlert);
        }

        private async Task<List<string>> GetPlatesToIgnoreAsync(CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.Ignores.SelectAsync(x => x.PlateNumber, cancellationToken);
        }

        private async Task<List<string>> GetPlatesToAlertAsync(CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.Alerts.SelectAsync(x => x.PlateNumber, cancellationToken);
        }
    }
} 