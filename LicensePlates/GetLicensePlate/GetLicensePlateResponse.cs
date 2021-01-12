using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates.GetLicensePlate
{
    public class GetLicensePlateResponse
    {
        public List<LicensePlate> Plates { get; set; }

        public int TotalCount { get; set; }
    }
}
