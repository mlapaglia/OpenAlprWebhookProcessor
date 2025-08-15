export default function () {
  return {
    name: 'dotnet-logs',
    contains: [
      { className: 'log-timestamp', begin: /\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2}/ },
      { className: 'log-http', begin: /HTTP\/\d\.\d\s+(GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS)\s+https?:\/\/[^\s]+/ },
      { className: 'log-duration', begin: /\d+(\.\d+)?\s?(ms|s|m|h)\b/ },
      { className: 'log-class', begin: /[A-Z][a-zA-Z0-9]*(\.[A-Z][a-zA-Z0-9]*){2,}/ },
      { className: 'log-assembly', begin: /\([^)]*\.[A-Z][a-zA-Z0-9]*\)/ },
      { className: 'log-trace', begin: /\[TRC\]/ },
      { className: 'log-debug', begin: /\[DBG\]/ },
      { className: 'log-info', begin: /\[INF\]/ },
      { className: 'log-warn', begin: /\[WRN\]/ },
      { className: 'log-error', begin: /\[ERR\]/ },
      { className: 'log-critical', begin: /\[CRT\]/ },
    ],
  };
}
