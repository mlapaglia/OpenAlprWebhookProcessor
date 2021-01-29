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

        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}
