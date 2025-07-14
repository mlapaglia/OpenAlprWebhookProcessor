using MediatR;
using System;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetMostSeenPlates
{
    public class GetMostSeenPlatesQuery : IRequest<GetMostSeenPlatesResponse>
    {
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public int Limit { get; set; } = 10;

        public GetMostSeenPlatesQuery(DateTimeOffset? startDate, DateTimeOffset? endDate, int limit = 10)
        {
            StartDate = startDate;
            EndDate = endDate;
            Limit = limit;
        }
    }
} 