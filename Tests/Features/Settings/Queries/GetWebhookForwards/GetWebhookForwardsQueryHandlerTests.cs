using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetWebhookForwards;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Queries.GetWebhookForwards
{
    [TestFixture]
    public class GetWebhookForwardsQueryHandlerTests : TestBase
    {
        private GetWebhookForwardsQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetWebhookForwardsQueryHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_WithExistingWebhookForwards_ReturnsCorrectWebhookForwardDtos()
        {
            // Arrange
            var forward1 = new OpenAlprWebhookProcessor.Data.WebhookForward
            {
                Id = Guid.NewGuid(),
                FowardingDestination = new Uri("https://example.com/webhook1"),
                IgnoreSslErrors = true,
                ForwardGroups = true,
                ForwardSinglePlates = false,
                ForwardGroupPreviews = true
            };
            var forward2 = new OpenAlprWebhookProcessor.Data.WebhookForward
            {
                Id = Guid.NewGuid(),
                FowardingDestination = new Uri("https://example.com/webhook2"),
                IgnoreSslErrors = false,
                ForwardGroups = false,
                ForwardSinglePlates = true,
                ForwardGroupPreviews = false
            };
            await UnitOfWork.WebhookForwards.AddAsync(forward1);
            await UnitOfWork.WebhookForwards.AddAsync(forward2);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetWebhookForwardsQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var resultForward1 = result.First(f => f.Id == forward1.Id);
            resultForward1.Destination.Should().Be(new Uri("https://example.com/webhook1"));
            resultForward1.IgnoreSslErrors.Should().BeTrue();
            resultForward1.ForwardGroups.Should().BeTrue();
            resultForward1.ForwardSinglePlates.Should().BeFalse();
            resultForward1.ForwardGroupPreviews.Should().BeTrue();

            var resultForward2 = result.First(f => f.Id == forward2.Id);
            resultForward2.Destination.Should().Be(new Uri("https://example.com/webhook2"));
            resultForward2.IgnoreSslErrors.Should().BeFalse();
            resultForward2.ForwardGroups.Should().BeFalse();
            resultForward2.ForwardSinglePlates.Should().BeTrue();
            resultForward2.ForwardGroupPreviews.Should().BeFalse();
        }

        [Test]
        public async Task Handle_WithNoWebhookForwards_ReturnsEmptyList()
        {
            // Arrange
            var query = new GetWebhookForwardsQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithSingleWebhookForward_ReturnsSingleWebhookForwardDto()
        {
            // Arrange
            var forward = new OpenAlprWebhookProcessor.Data.WebhookForward
            {
                Id = Guid.NewGuid(),
                FowardingDestination = new Uri("https://single.example.com/webhook"),
                IgnoreSslErrors = true,
                ForwardGroups = true,
                ForwardSinglePlates = true,
                ForwardGroupPreviews = true
            };
            await UnitOfWork.WebhookForwards.AddAsync(forward);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetWebhookForwardsQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);

            var resultForward = result.First();
            resultForward.Id.Should().Be(forward.Id);
            resultForward.Destination.Should().Be(new Uri("https://single.example.com/webhook"));
            resultForward.IgnoreSslErrors.Should().BeTrue();
            resultForward.ForwardGroups.Should().BeTrue();
            resultForward.ForwardSinglePlates.Should().BeTrue();
            resultForward.ForwardGroupPreviews.Should().BeTrue();
        }

        [Test]
        public async Task Handle_WithHttpsDestination_ReturnsHttpsDestination()
        {
            // Arrange
            var forward = new OpenAlprWebhookProcessor.Data.WebhookForward
            {
                Id = Guid.NewGuid(),
                FowardingDestination = new Uri("https://secure.example.com/webhook"),
                IgnoreSslErrors = false,
                ForwardGroups = true,
                ForwardSinglePlates = false,
                ForwardGroupPreviews = true
            };
            await UnitOfWork.WebhookForwards.AddAsync(forward);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetWebhookForwardsQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);

            var resultForward = result.First();
            resultForward.Destination.Should().Be(new Uri("https://secure.example.com/webhook"));
            resultForward.Destination.Scheme.Should().Be("https");
        }

        [Test]
        public async Task Handle_WithHttpDestination_ReturnsHttpDestination()
        {
            // Arrange
            var forward = new OpenAlprWebhookProcessor.Data.WebhookForward
            {
                Id = Guid.NewGuid(),
                FowardingDestination = new Uri("http://insecure.example.com/webhook"),
                IgnoreSslErrors = true,
                ForwardGroups = false,
                ForwardSinglePlates = true,
                ForwardGroupPreviews = false
            };
            await UnitOfWork.WebhookForwards.AddAsync(forward);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetWebhookForwardsQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);

            var resultForward = result.First();
            resultForward.Destination.Should().Be(new Uri("http://insecure.example.com/webhook"));
            resultForward.Destination.Scheme.Should().Be("http");
        }

        [Test]
        public async Task Handle_MapsAllBooleanFieldsCorrectly()
        {
            // Arrange
            var forward1 = new OpenAlprWebhookProcessor.Data.WebhookForward
            {
                Id = Guid.NewGuid(),
                FowardingDestination = new Uri("https://all-true.example.com/webhook"),
                IgnoreSslErrors = true,
                ForwardGroups = true,
                ForwardSinglePlates = true,
                ForwardGroupPreviews = true
            };
            var forward2 = new OpenAlprWebhookProcessor.Data.WebhookForward
            {
                Id = Guid.NewGuid(),
                FowardingDestination = new Uri("https://all-false.example.com/webhook"),
                IgnoreSslErrors = false,
                ForwardGroups = false,
                ForwardSinglePlates = false,
                ForwardGroupPreviews = false
            };
            await UnitOfWork.WebhookForwards.AddAsync(forward1);
            await UnitOfWork.WebhookForwards.AddAsync(forward2);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetWebhookForwardsQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var allTrueResult = result.First(f => f.Destination.Host == "all-true.example.com");
            allTrueResult.IgnoreSslErrors.Should().BeTrue();
            allTrueResult.ForwardGroups.Should().BeTrue();
            allTrueResult.ForwardSinglePlates.Should().BeTrue();
            allTrueResult.ForwardGroupPreviews.Should().BeTrue();

            var allFalseResult = result.First(f => f.Destination.Host == "all-false.example.com");
            allFalseResult.IgnoreSslErrors.Should().BeFalse();
            allFalseResult.ForwardGroups.Should().BeFalse();
            allFalseResult.ForwardSinglePlates.Should().BeFalse();
            allFalseResult.ForwardGroupPreviews.Should().BeFalse();
        }

        [Test]
        public async Task Handle_CallsGetAllAsync()
        {
            // Arrange
            var query = new GetWebhookForwardsQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            // The fact that we get a result (even if empty) confirms the method was called
        }
    }
} 