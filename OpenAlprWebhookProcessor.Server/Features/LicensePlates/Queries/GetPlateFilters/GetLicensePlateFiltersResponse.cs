using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlateFilters
{
    public class GetLicensePlateFiltersResponse
    {
        public List<string> VehicleMakes { get; set; }

        public List<string> VehicleModels { get; set; }

        public List<string> VehicleTypes { get; set; }

        public List<string> VehicleYears { get; set; }

        public List<string> VehicleColors { get; set; }

        public List<string> VehicleRegions { get; set; }
    }
}
