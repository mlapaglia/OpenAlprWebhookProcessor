using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Server.LicensePlates.GetMostSeenPlates
{
    public class GetMostSeenPlatesResponse
    {
        public List<MostSeenCount> Counts { get; set; }
    }
}
