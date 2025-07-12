using MediatR;
using OpenAlprWebhookProcessor.Server.Features.LicensePlates.Queries.GetLicensePlateCounts;
using System;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts
{
    public class GetLicensePlateCountsQuery : IRequest<GetLicensePlateCountsResponse>
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public GetLicensePlateCountsQuery(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }
    }
} 