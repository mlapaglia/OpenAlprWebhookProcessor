using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Server.Features;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlate
{
    public class GetPlateQueryHandler : IRequestHandler<GetPlateQuery, LicensePlate?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPlateQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<LicensePlate?> Handle(GetPlateQuery request, CancellationToken cancellationToken)
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