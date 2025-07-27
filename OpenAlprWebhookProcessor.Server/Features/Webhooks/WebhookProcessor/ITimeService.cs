using System;

namespace OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor
{
    public interface ITimeService
    {
        DateTimeOffset UtcNow { get; }
        long UtcNowMilliseconds { get; }
    }
    
    public class TimeService : ITimeService
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
        public long UtcNowMilliseconds => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
} 