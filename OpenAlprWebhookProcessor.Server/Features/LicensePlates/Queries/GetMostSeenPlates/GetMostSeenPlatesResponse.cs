using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Server.Features.LicensePlates.Queries.GetMostSeenPlates
{
    public class GetMostSeenPlatesResponse
    {
        public List<MostSeenCount> Counts { get; set; }
    }
}
