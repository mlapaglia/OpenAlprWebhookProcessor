using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts
{
    public class GetLicensePlateCountsResponse
    {
        public List<DayCount> Counts { get; set; }
    }
}
