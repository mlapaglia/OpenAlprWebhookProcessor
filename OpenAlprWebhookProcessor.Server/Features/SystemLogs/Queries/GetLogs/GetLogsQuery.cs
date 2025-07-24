using MediatR;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.SystemLogs.Queries.GetLogs
{
    public class GetLogsQuery : IRequest<List<string>>
    {
        public ApiLogLevel MinimumSeverity { get; set; }

        public GetLogsQuery(ApiLogLevel minimumSeverity)
        {
            MinimumSeverity = minimumSeverity;
        }
    }
} 