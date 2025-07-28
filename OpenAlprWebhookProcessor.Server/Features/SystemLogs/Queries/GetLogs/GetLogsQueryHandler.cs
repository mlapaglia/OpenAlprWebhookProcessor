using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.SystemLogs.Queries.GetLogs
{
    public partial class GetLogsQueryHandler : IRequestHandler<GetLogsQuery, List<string>>
    {
        private readonly IFileSystem _fileSystem;

        public GetLogsQueryHandler(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public async Task<List<string>> Handle(
            GetLogsQuery request,
            CancellationToken cancellationToken = default)
        {
            var currentLogFile = _fileSystem.Directory.GetFiles("./config/")
                .LastOrDefault(x => x.Contains("log-"));
            if (currentLogFile == null)
            {
                return new List<string>();
            }

            using var stream = _fileSystem.File.Open(
                currentLogFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);
            using var sr = new StreamReader(stream, Encoding.UTF8);

            var logEntries = await ParseLogEntriesAsync(sr, cancellationToken);

            var filteredLogs = logEntries
                .Where(entry => ShouldIncludeLogEntry(entry, request.MinimumSeverity))
                .Where(entry => ShouldIncludeLogEntryBySearch(entry, request.SearchString))
                .Take(500)
                .Reverse()
                .ToList();

            return filteredLogs;
        }

        private static async Task<List<string>> ParseLogEntriesAsync(
            StreamReader sr,
            CancellationToken cancellationToken = default)
        {
            var logEntries = new List<string>();
            var currentEntry = new StringBuilder();

            while (!sr.EndOfStream)
            {
                var line = await sr.ReadLineAsync(cancellationToken);
                if (line == null) continue;

                if (IsNewLogEntry(line))
                {
                    if (currentEntry.Length > 0)
                    {
                        logEntries.Add(currentEntry.ToString().TrimEnd());
                        currentEntry.Clear();
                    }

                    currentEntry.AppendLine(line);
                }
                else
                {
                    if (currentEntry.Length > 0)
                    {
                        currentEntry.AppendLine(line);
                    }
                }
            }

            if (currentEntry.Length > 0)
            {
                logEntries.Add(currentEntry.ToString().TrimEnd());
            }

            return logEntries;
        }

        private static bool IsNewLogEntry(string line)
        {
            return TimestampAndLogLevel().IsMatch(line);
        }

        private static bool ShouldIncludeLogEntry(
            string logEntry,
            ApiLogLevel? minimumSeverity)
        {
            if (minimumSeverity == null) return true;

            var firstLine = logEntry.Split('\n')[0];
            var logLevel = ExtractLogLevel(firstLine);

            if (logLevel == null) return true;
            return logLevel >= minimumSeverity;
        }

        private static ApiLogLevel? ExtractLogLevel(string line)
        {
            var match = LogLevel().Match(line);
            if (!match.Success) return null;

            return match.Groups[1].Value switch
            {
                "VRB" => ApiLogLevel.Verbose,
                "DBG" => ApiLogLevel.Debug,
                "INF" => ApiLogLevel.Information,
                "WRN" => ApiLogLevel.Warning,
                "ERR" => ApiLogLevel.Error,
                "FTL" => ApiLogLevel.Critical,
                _ => null
            };
        }

        private static bool ShouldIncludeLogEntryBySearch(string logEntry, string searchString)
        {
            if (string.IsNullOrEmpty(searchString)) return true;
            return logEntry.Contains(searchString, StringComparison.OrdinalIgnoreCase);
        }

        [System.Text.RegularExpressions.GeneratedRegex(@"\[([A-Z]{3})\]")]
        private static partial System.Text.RegularExpressions.Regex LogLevel();

        [System.Text.RegularExpressions.GeneratedRegex(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [-+]\d{2}:\d{2} \[[A-Z]{3}\]")]
        private static partial System.Text.RegularExpressions.Regex TimestampAndLogLevel();
    }
}