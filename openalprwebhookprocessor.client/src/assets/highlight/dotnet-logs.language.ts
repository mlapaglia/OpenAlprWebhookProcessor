import { HLJSApi } from 'highlight.js';

export default function(hljs: HLJSApi) {
  return {
    name: 'dotnet-logs',
    contains: [
      // Timestamps - ISO 8601 format with timezone
      {
        className: 'log-timestamp',
        begin: /\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2}/
      },
      // HTTP requests - highlight in blue
      {
        className: 'log-http',
        begin: /HTTP\/\d\.\d\s+(GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS)\s+https?:\/\/[^\s]+/
      },
      // Time intervals - milliseconds, seconds, minutes (with or without space)
      {
        className: 'log-duration',
        begin: /\d+(\.\d+)?\s?(ms|s|m|h)\b/
      },
      // .NET class names - namespace.class.method pattern
      {
        className: 'log-class',
        begin: /[A-Z][a-zA-Z0-9]*(\.[A-Z][a-zA-Z0-9]*){2,}/
      },
      // Assembly names in parentheses
      {
        className: 'log-assembly',
        begin: /\([^)]*\.[A-Z][a-zA-Z0-9]*\)/
      },
      // TRACE - gray
      {
        className: 'log-trace',
        begin: /\[TRC\]/
      },
      // DEBUG - blue
      {
        className: 'log-debug', 
        begin: /\[DBG\]/
      },
      // INFO - green/teal
      {
        className: 'log-info',
        begin: /\[INF\]/
      },
      // WARN - yellow/orange
      {
        className: 'log-warn',
        begin: /\[WRN\]/
      },
      // ERROR - red
      {
        className: 'log-error',
        begin: /\[ERR\]/
      },
      // CRITICAL - bright red with background
      {
        className: 'log-critical',
        begin: /\[CRT\]/
      }
    ]
  };
}