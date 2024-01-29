using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.LicensePlates.SearchLicensePlates
{
    public class SearchLicensePlateResponse
    {
        public List<LicensePlate> Plates { get; set; }

        public int TotalCount { get; set; }
    }
}
