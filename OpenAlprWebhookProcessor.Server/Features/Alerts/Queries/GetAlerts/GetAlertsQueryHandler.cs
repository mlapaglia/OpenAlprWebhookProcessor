using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Queries.GetAlerts
{
    public class GetAlertsQueryHandler : IRequestHandler<GetAlertsQuery, List<Alert>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAlertsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Alert>> Handle(
            GetAlertsQuery request,
            CancellationToken cancellationToken)
        {
            var dbAlerts = await _unitOfWork.Alerts.GetAllAsync(cancellationToken);

            return dbAlerts.Select(x => new Alert
            {
                Id = x.Id,
                PlateNumber = x.PlateNumber,
                Description = x.Description,
                StrictMatch = x.IsStrictMatch
            }).ToList();
        }
    }
} 