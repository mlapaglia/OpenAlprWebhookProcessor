using MediatR;
using OpenAlprWebhookProcessor.Server.Features.LicensePlates.Queries.GetStatistics;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetStatistics
{
    public class GetStatisticsQuery : IRequest<PlateStatistics>
    {
        public string PlateNumber { get; set; } = string.Empty;
        
        public GetStatisticsQuery(string plateNumber)
        {
            PlateNumber = plateNumber;
        }
    }
} 