using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Settings.Commands.AgentScrape;
using OpenAlprWebhookProcessor.Hydrator;
using System;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Commands.AgentScrape
{
    [TestFixture]
    public class AgentScrapeCommandHandlerTests : TestBase
    {
        private AgentScrapeCommandHandler _handler;
        private IHydrationService _hydrationService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _hydrationService = Substitute.For<IHydrationService>();
            _handler = new AgentScrapeCommandHandler(_hydrationService);
        }

        [Test]
        public async Task Handle_ValidCommand_CallsHydrationService()
        {
            // Arrange
            var command = new AgentScrapeCommand();

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            _hydrationService.Received(1).StartHydration("hydration");
        }

        [Test]
        public async Task Handle_ValidCommand_CompletesSuccessfully()
        {
            // Arrange
            var command = new AgentScrapeCommand();

            // Act
            var task = _handler.Handle(command, GetCancellationToken());

            // Assert
            task.Should().NotBeNull();
            task.IsCompleted.Should().BeTrue();
            task.Status.Should().Be(TaskStatus.RanToCompletion);
        }

        [Test]
        public async Task Handle_HydrationServiceThrowsException_ExceptionBubbles()
        {
            // Arrange
            var command = new AgentScrapeCommand();
            _hydrationService.When(x => x.StartHydration("hydration"))
                .Do(x => throw new InvalidOperationException("Test exception"));

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, GetCancellationToken()));
        }

        [Test]
        public async Task Handle_ValidCommand_CallsStartHydrationWithCorrectParameter()
        {
            // Arrange
            var command = new AgentScrapeCommand();

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            _hydrationService.Received(1).StartHydration(Arg.Is<string>(s => s == "hydration"));
        }
    }
} 