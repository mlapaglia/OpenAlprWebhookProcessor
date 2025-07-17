using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertEnrichers;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetEnrichers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Commands.UpsertEnrichers
{
    [TestFixture]
    public class UpsertEnrichersCommandHandlerTests : TestBase
    {
        private UpsertEnrichersCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new UpsertEnrichersCommandHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_NewEnricher_AddsEnricherToDatabase()
        {
            // Arrange
            var enricherDto = new EnricherDto
            {
                Id = Guid.NewGuid(),
                ApiKey = "test-api-key",
                EnricherType = EnricherType.LicenseDateDataApi,
                IsEnabled = true,
                EnrichmentType = EnrichmentType.Always
            };
            var command = new UpsertEnrichersCommand(enricherDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbEnrichers = await UnitOfWork.Enrichers.GetAllAsync();
            dbEnrichers.Should().HaveCount(1);

            var dbEnricher = dbEnrichers.First();
            dbEnricher.ApiKey.Should().Be("test-api-key");
            dbEnricher.EnricherType.Should().Be(EnricherType.LicenseDateDataApi);
            dbEnricher.IsEnabled.Should().BeTrue();
            dbEnricher.EnrichmentType.Should().Be(EnrichmentType.Always);
        }

        [Test]
        public async Task Handle_ExistingEnricher_UpdatesEnricherInDatabase()
        {
            // Arrange
            var enricherId = Guid.NewGuid();
            var existingEnricher = new OpenAlprWebhookProcessor.Data.Enricher
            {
                Id = enricherId,
                ApiKey = "old-api-key",
                EnricherType = EnricherType.LicenseDateDataApi,
                IsEnabled = false,
                EnrichmentType = EnrichmentType.Always
            };
            await UnitOfWork.Enrichers.AddAsync(existingEnricher);
            await UnitOfWork.SaveChangesAsync();

            var enricherDto = new EnricherDto
            {
                Id = enricherId,
                ApiKey = "new-api-key",
                EnricherType = EnricherType.LicenseDateDataApi,
                IsEnabled = true,
                EnrichmentType = EnrichmentType.Always
            };
            var command = new UpsertEnrichersCommand(enricherDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbEnrichers = await UnitOfWork.Enrichers.GetAllAsync();
            dbEnrichers.Should().HaveCount(1);

            var dbEnricher = dbEnrichers.First();
            dbEnricher.Id.Should().Be(enricherId);
            dbEnricher.ApiKey.Should().Be("new-api-key");
            dbEnricher.IsEnabled.Should().BeTrue();
        }

        [Test]
        public async Task Handle_NewEnricher_SavesChanges()
        {
            // Arrange
            var enricherDto = new EnricherDto
            {
                Id = Guid.NewGuid(),
                ApiKey = "test-api-key",
                EnricherType = EnricherType.LicenseDateDataApi,
                IsEnabled = true,
                EnrichmentType = EnrichmentType.Always
            };
            var command = new UpsertEnrichersCommand(enricherDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbEnrichers = await UnitOfWork.Enrichers.GetAllAsync();
            dbEnrichers.Should().HaveCount(1);
        }

        [Test]
        public async Task Handle_ExistingEnricher_SavesChanges()
        {
            // Arrange
            var enricherId = Guid.NewGuid();
            var existingEnricher = new OpenAlprWebhookProcessor.Data.Enricher
            {
                Id = enricherId,
                ApiKey = "old-api-key",
                EnricherType = EnricherType.LicenseDateDataApi,
                IsEnabled = false,
                EnrichmentType = EnrichmentType.Always
            };
            await UnitOfWork.Enrichers.AddAsync(existingEnricher);
            await UnitOfWork.SaveChangesAsync();

            var enricherDto = new EnricherDto
            {
                Id = enricherId,
                ApiKey = "new-api-key",
                EnricherType = EnricherType.LicenseDateDataApi,
                IsEnabled = true,
                EnrichmentType = EnrichmentType.Always
            };
            var command = new UpsertEnrichersCommand(enricherDto);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbEnrichers = await UnitOfWork.Enrichers.GetAllAsync();
            dbEnrichers.Should().HaveCount(1);
            var dbEnricher = dbEnrichers.First();
            dbEnricher.ApiKey.Should().Be("new-api-key");
        }
    }
} 