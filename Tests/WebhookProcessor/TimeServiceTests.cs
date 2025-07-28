using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;

namespace Tests.WebhookProcessor
{
    [TestFixture]
    public class TimeServiceTests
    {
        private TimeService _timeService;

        [SetUp]
        public void SetUp()
        {
            _timeService = new TimeService();
        }

        [Test]
        public void UtcNow_ReturnsCurrentTime()
        {
            // Arrange
            var beforeCall = DateTimeOffset.UtcNow;

            // Act
            var result = _timeService.UtcNow;

            // Assert
            var afterCall = DateTimeOffset.UtcNow;
            result.Should().BeOnOrAfter(beforeCall);
            result.Should().BeOnOrBefore(afterCall);
        }

        [Test]
        public void UtcNowMilliseconds_ReturnsCurrentTimeInMilliseconds()
        {
            // Arrange
            var beforeCall = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Act
            var result = _timeService.UtcNowMilliseconds;

            // Assert
            var afterCall = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            result.Should().BeGreaterThanOrEqualTo(beforeCall);
            result.Should().BeLessThanOrEqualTo(afterCall);
        }

        [Test]
        public void UtcNowMilliseconds_MatchesUtcNowConversion()
        {
            // Act
            var utcNow = _timeService.UtcNow;
            var utcNowMilliseconds = _timeService.UtcNowMilliseconds;

            // Assert
            var expectedMilliseconds = utcNow.ToUnixTimeMilliseconds();
            // Allow for small difference due to time passing between calls
            Math.Abs(utcNowMilliseconds - expectedMilliseconds).Should().BeLessThan(100);
        }
    }
} 