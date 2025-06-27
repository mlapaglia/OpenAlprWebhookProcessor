using OpenAlprWebhookProcessor.Server.LicensePlates;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Server.LicensePlates.SearchLicensePlates
{
    public class SearchLicensePlateResponse
    {
        public List<LicensePlate> Plates { get; set; }

        public int TotalCount { get; set; }
    }
}
