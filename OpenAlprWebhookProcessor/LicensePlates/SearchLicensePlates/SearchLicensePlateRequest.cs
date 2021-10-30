using System;

namespace OpenAlprWebhookProcessor.LicensePlates.SearchLicensePlates
{
    public class SearchLicensePlateRequest
    {
        public string PlateNumber { get; set; }

        public DateTimeOffset? StartSearchOn { get; set; }

        public DateTimeOffset? EndSearchOn { get; set; }

        public bool StrictMatch { get; set; }

        public bool FilterIgnoredPlates { get; set; }

        public bool RegexSearchEnabled { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public string VehicleMake { get; set; }

        public string VehicleModel { get; set; }

        public string VehicleType { get; set; }

        public string VehicleYear { get; set; }

        public string VehicleRegion { get; set; }

        public int? FilterPlatesSeenLessThan { get; set; } = 0;
    }
}
