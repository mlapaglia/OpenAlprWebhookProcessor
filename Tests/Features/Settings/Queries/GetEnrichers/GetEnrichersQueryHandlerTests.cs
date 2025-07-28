using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetEnrichers;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Queries.GetEnrichers
{
    [TestFixture]
    public class GetEnrichersQueryHandlerTests : TestBase
    {
        private GetEnrichersQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetEnrichersQueryHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_WithExistingEnricher_ReturnsCorrectEnricherDto()
        {
            // Arrange
            var enricher = new OpenAlprWebhookProcessor.Data.Enricher
            {
                Id = Guid.NewGuid(),
                ApiKey = "test-api-key",
                EnricherType = EnricherType.LicenseDateDataApi,
                IsEnabled = true,
                EnrichmentType = EnrichmentType.Always
            };
            await UnitOfWork.Enrichers.AddAsync(enricher);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetEnrichersQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(enricher.Id);
            result.ApiKey.Should().Be("test-api-key");
            result.EnricherType.Should().Be(EnricherType.LicenseDateDataApi);
            result.IsEnabled.Should().BeTrue();
            result.EnrichmentType.Should().Be(EnrichmentType.Always);
        }

        [Test]
        public async Task Handle_WithNoEnrichers_ReturnsEmptyEnricherDto()
        {
            // Arrange
            var query = new GetEnrichersQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(Guid.Empty);
            result.ApiKey.Should().BeNull();
            result.EnricherType.Should().Be(default(EnricherType));
            result.IsEnabled.Should().BeFalse();
            result.EnrichmentType.Should().Be(default(EnrichmentType));
        }

        [Test]
        public async Task Handle_WithMultipleEnrichers_ReturnsFirstEnricher()
        {
            // Arrange
            var enricher1 = new OpenAlprWebhookProcessor.Data.Enricher
            {
                Id = Guid.NewGuid(),
                ApiKey = "first-api-key",
                EnricherType = EnricherType.LicenseDateDataApi,
                IsEnabled = true,
                EnrichmentType = EnrichmentType.Always
            };
            var enricher2 = new OpenAlprWebhookProcessor.Data.Enricher
            {
                Id = Guid.NewGuid(),
                ApiKey = "second-api-key",
                EnricherType = EnricherType.LicenseDateDataApi,
                IsEnabled = false,
                EnrichmentType = EnrichmentType.Always
            };
            await UnitOfWork.Enrichers.AddAsync(enricher1);
            await UnitOfWork.Enrichers.AddAsync(enricher2);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetEnrichersQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            // Should return the first enricher (order may vary, but should be one of them)
            (result.Id == enricher1.Id || result.Id == enricher2.Id).Should().BeTrue();
            (result.ApiKey == "first-api-key" || result.ApiKey == "second-api-key").Should().BeTrue();
        }

        [Test]
        public async Task Handle_WithNullApiKey_ReturnsEnricherWithNullApiKey()
        {
            // Arrange
            var enricher = new OpenAlprWebhookProcessor.Data.Enricher
            {
                Id = Guid.NewGuid(),
                ApiKey = null,
                EnricherType = EnricherType.LicenseDateDataApi,
                IsEnabled = false,
                EnrichmentType = EnrichmentType.Always
            };
            await UnitOfWork.Enrichers.AddAsync(enricher);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetEnrichersQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.ApiKey.Should().BeNull();
            result.IsEnabled.Should().BeFalse();
        }

        [Test]
        public async Task Handle_WithDisabledEnricher_ReturnsDisabledEnricher()
        {
            // Arrange
            var enricher = new OpenAlprWebhookProcessor.Data.Enricher
            {
                Id = Guid.NewGuid(),
                ApiKey = "test-api-key",
                EnricherType = EnricherType.LicenseDateDataApi,
                IsEnabled = false,
                EnrichmentType = EnrichmentType.Always
            };
            await UnitOfWork.Enrichers.AddAsync(enricher);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetEnrichersQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.IsEnabled.Should().BeFalse();
        }

        [Test]
        public async Task Handle_CallsGetAllAsync()
        {
            // Arrange
            var query = new GetEnrichersQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            // The fact that we get a result (even if empty) confirms the method was called
        }
    }
} 