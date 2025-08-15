using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertWebhookForwards;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetWebhookForwards;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Commands.UpsertWebhookForwards
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UpsertWebhookForwardsCommandHandlerTests : TestBase
    {
        private UpsertWebhookForwardsCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new UpsertWebhookForwardsCommandHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_NewWebhookForwards_AddsForwardsToDatabase()
        {
            // Arrange
            var forwards = new List<WebhookForwardDto>
            {
                new WebhookForwardDto
                {
                    Id = Guid.NewGuid(),
                    Destination = new Uri("https://example.com/webhook1"),
                    IgnoreSslErrors = true,
                    ForwardGroups = true,
                    ForwardSinglePlates = false,
                    ForwardGroupPreviews = true
                },
                new WebhookForwardDto
                {
                    Id = Guid.NewGuid(),
                    Destination = new Uri("https://example.com/webhook2"),
                    IgnoreSslErrors = false,
                    ForwardGroups = false,
                    ForwardSinglePlates = true,
                    ForwardGroupPreviews = false
                }
            };
            var command = new UpsertWebhookForwardsCommand(forwards);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbForwards = await UnitOfWork.WebhookForwards.GetAllAsync();
            dbForwards.Should().HaveCount(2);

            var destinations = dbForwards.Select(f => f.FowardingDestination.ToString()).ToList();
            destinations.Should().Contain("https://example.com/webhook1");
            destinations.Should().Contain("https://example.com/webhook2");
        }

        [Test]
        public async Task Handle_UpdateExistingForwards_UpdatesForwardsInDatabase()
        {
            // Arrange
            var forwardId = Guid.NewGuid();
            var existingForward = new OpenAlprWebhookProcessor.Data.WebhookForward
            {
                Id = forwardId,
                FowardingDestination = new Uri("https://old.example.com"),
                IgnoreSslErrors = false,
                ForwardGroups = false,
                ForwardSinglePlates = false,
                ForwardGroupPreviews = false
            };
            await UnitOfWork.WebhookForwards.AddAsync(existingForward);
            await UnitOfWork.SaveChangesAsync();

            var forwards = new List<WebhookForwardDto>
            {
                new WebhookForwardDto
                {
                    Id = forwardId,
                    Destination = new Uri("https://new.example.com"),
                    IgnoreSslErrors = true,
                    ForwardGroups = true,
                    ForwardSinglePlates = true,
                    ForwardGroupPreviews = true
                }
            };
            var command = new UpsertWebhookForwardsCommand(forwards);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbForwards = await UnitOfWork.WebhookForwards.GetAllAsync();
            dbForwards.Should().HaveCount(1);

            var dbForward = dbForwards.First();
            dbForward.Id.Should().Be(forwardId);
            dbForward.FowardingDestination.Should().Be(new Uri("https://new.example.com"));
            dbForward.IgnoreSslErrors.Should().BeTrue();
            dbForward.ForwardGroups.Should().BeTrue();
            dbForward.ForwardSinglePlates.Should().BeTrue();
            dbForward.ForwardGroupPreviews.Should().BeTrue();
        }

        [Test]
        public async Task Handle_RemoveForwards_DeletesForwardsFromDatabase()
        {
            // Arrange
            var forwardToKeep = new OpenAlprWebhookProcessor.Data.WebhookForward
            {
                Id = Guid.NewGuid(),
                FowardingDestination = new Uri("https://keep.example.com"),
                IgnoreSslErrors = false,
                ForwardGroups = true,
                ForwardSinglePlates = false,
                ForwardGroupPreviews = true
            };
            var forwardToRemove = new OpenAlprWebhookProcessor.Data.WebhookForward
            {
                Id = Guid.NewGuid(),
                FowardingDestination = new Uri("https://remove.example.com"),
                IgnoreSslErrors = true,
                ForwardGroups = false,
                ForwardSinglePlates = true,
                ForwardGroupPreviews = false
            };
            await UnitOfWork.WebhookForwards.AddAsync(forwardToKeep);
            await UnitOfWork.WebhookForwards.AddAsync(forwardToRemove);
            await UnitOfWork.SaveChangesAsync();

            var forwards = new List<WebhookForwardDto>
            {
                new WebhookForwardDto
                {
                    Id = forwardToKeep.Id,
                    Destination = new Uri("https://keep.example.com"),
                    IgnoreSslErrors = false,
                    ForwardGroups = true,
                    ForwardSinglePlates = false,
                    ForwardGroupPreviews = true
                }
            };
            var command = new UpsertWebhookForwardsCommand(forwards);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbForwards = await UnitOfWork.WebhookForwards.GetAllAsync();
            dbForwards.Should().HaveCount(1);
            dbForwards.First().FowardingDestination.Should().Be(new Uri("https://keep.example.com"));
        }

        [Test]
        public async Task Handle_NullDestinations_FiltersOutInvalidForwards()
        {
            // Arrange
            var forwards = new List<WebhookForwardDto>
            {
                new WebhookForwardDto
                {
                    Id = Guid.NewGuid(),
                    Destination = new Uri("https://valid.example.com"),
                    IgnoreSslErrors = false,
                    ForwardGroups = true,
                    ForwardSinglePlates = false,
                    ForwardGroupPreviews = true
                },
                new WebhookForwardDto
                {
                    Id = Guid.NewGuid(),
                    Destination = null,
                    IgnoreSslErrors = false,
                    ForwardGroups = false,
                    ForwardSinglePlates = true,
                    ForwardGroupPreviews = false
                }
            };
            var command = new UpsertWebhookForwardsCommand(forwards);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbForwards = await UnitOfWork.WebhookForwards.GetAllAsync();
            dbForwards.Should().HaveCount(1);
            dbForwards.First().FowardingDestination.Should().Be(new Uri("https://valid.example.com"));
        }

        [Test]
        public async Task Handle_MixedAddUpdateRemove_HandlesAllOperations()
        {
            // Arrange
            var existingForwardId = Guid.NewGuid();
            var forwardToRemoveId = Guid.NewGuid();
            
            var existingForward = new OpenAlprWebhookProcessor.Data.WebhookForward
            {
                Id = existingForwardId,
                FowardingDestination = new Uri("https://update.example.com"),
                IgnoreSslErrors = false,
                ForwardGroups = false,
                ForwardSinglePlates = false,
                ForwardGroupPreviews = false
            };
            var forwardToRemove = new OpenAlprWebhookProcessor.Data.WebhookForward
            {
                Id = forwardToRemoveId,
                FowardingDestination = new Uri("https://remove.example.com"),
                IgnoreSslErrors = true,
                ForwardGroups = true,
                ForwardSinglePlates = true,
                ForwardGroupPreviews = true
            };
            await UnitOfWork.WebhookForwards.AddAsync(existingForward);
            await UnitOfWork.WebhookForwards.AddAsync(forwardToRemove);
            await UnitOfWork.SaveChangesAsync();

            var forwards = new List<WebhookForwardDto>
            {
                new WebhookForwardDto
                {
                    Id = existingForwardId,
                    Destination = new Uri("https://updated.example.com"),
                    IgnoreSslErrors = true,
                    ForwardGroups = true,
                    ForwardSinglePlates = true,
                    ForwardGroupPreviews = true
                },
                new WebhookForwardDto
                {
                    Id = Guid.NewGuid(),
                    Destination = new Uri("https://new.example.com"),
                    IgnoreSslErrors = false,
                    ForwardGroups = false,
                    ForwardSinglePlates = false,
                    ForwardGroupPreviews = false
                }
            };
            var command = new UpsertWebhookForwardsCommand(forwards);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbForwards = await UnitOfWork.WebhookForwards.GetAllAsync();
            dbForwards.Should().HaveCount(2);

            var destinations = dbForwards.Select(f => f.FowardingDestination.ToString()).ToList();
            destinations.Should().Contain("https://updated.example.com/");
            destinations.Should().Contain("https://new.example.com/");
            destinations.Should().NotContain("https://remove.example.com");
        }
    }
} 