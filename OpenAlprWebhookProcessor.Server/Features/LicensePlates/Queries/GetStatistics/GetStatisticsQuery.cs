using Mediator;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetStatistics
{
    public class GetStatisticsQuery : IQuery<PlateStatistics>
    {
        public string PlateNumber { get; set; } = string.Empty;
        
        public GetStatisticsQuery(string plateNumber)
        {
            PlateNumber = plateNumber;
        }
    }
} 