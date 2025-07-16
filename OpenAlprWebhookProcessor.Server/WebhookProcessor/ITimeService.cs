using System;

namespace OpenAlprWebhookProcessor.WebhookProcessor
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