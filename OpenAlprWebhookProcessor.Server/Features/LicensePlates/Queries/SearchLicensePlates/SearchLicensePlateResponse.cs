using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Server.Features.LicensePlates.Queries.SearchLicensePlates
{
    public class SearchLicensePlateResponse
    {
        public List<LicensePlate> Plates { get; set; }

        public int TotalCount { get; set; }
    }
}
