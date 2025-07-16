using MediatR;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.SystemLogs.Queries.GetLogs
{
    public class GetLogsQueryHandler : IRequestHandler<GetLogsQuery, List<string>>
    {
        public GetLogsQueryHandler()
        {
        }

        public async Task<List<string>> Handle(GetLogsQuery request, CancellationToken cancellationToken)
        {
            var currentLogFile = Directory.GetFiles("./config/")
                .LastOrDefault(x => x.Contains("log-"));

            if (currentLogFile == null)
            {
                return new List<string>();
            }

            using (var stream = File.Open(
                currentLogFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(stream, Encoding.UTF8))
                {
                    var logs = new List<string>();

                    while (!sr.EndOfStream)
                    {
                        var line = await sr.ReadLineAsync();
                        if (line != null)
                        {
                            logs.Add(line);
                        }
                    }

                    logs.Reverse();
                    return logs.Take(500).ToList();
                }
            }
        }
    }
} 