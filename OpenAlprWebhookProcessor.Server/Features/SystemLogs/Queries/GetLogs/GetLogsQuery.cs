using Mediator;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.SystemLogs.Queries.GetLogs
{
    public class GetLogsQuery : IQuery<List<string>>
    {
        public ApiLogLevel MinimumSeverity { get; set; }
        
        public string? SearchString { get; set; }

        public GetLogsQuery(ApiLogLevel minimumSeverity, string? searchString = null)
        {
            MinimumSeverity = minimumSeverity;
            SearchString = searchString;
        }
    }
} 