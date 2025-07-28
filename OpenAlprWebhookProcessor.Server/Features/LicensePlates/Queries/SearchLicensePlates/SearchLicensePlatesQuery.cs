using Mediator;
using System;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates
{
    public class SearchLicensePlatesQuery : IQuery<SearchLicensePlateResponse>
    {
        public string? PlateNumber { get; set; }
        public bool StrictMatch { get; set; }
        public bool RegexSearchEnabled { get; set; }
        public DateTimeOffset? StartSearchOn { get; set; }
        public DateTimeOffset? EndSearchOn { get; set; }
        public bool FilterIgnoredPlates { get; set; }
        public string? VehicleColor { get; set; }
        public string? VehicleMake { get; set; }
        public string? VehicleModel { get; set; }
        public string? VehicleType { get; set; }
        public string? VehicleRegion { get; set; }
        public int FilterPlatesSeenLessThan { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
} 