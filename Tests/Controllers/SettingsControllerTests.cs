using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Settings;
using OpenAlprWebhookProcessor.Features.Settings.Commands.AddIgnore;
using OpenAlprWebhookProcessor.Features.Settings.Commands.AgentScrape;
using OpenAlprWebhookProcessor.Features.Settings.Commands.DeleteDebugPlates;
using OpenAlprWebhookProcessor.Features.Settings.Commands.DisableAgent;
using OpenAlprWebhookProcessor.Features.Settings.Commands.EnableAgent;
using OpenAlprWebhookProcessor.Features.Settings.Commands.TestEnrichers;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertAgent;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertEnrichers;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertIgnores;
using OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertWebhookForwards;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgent;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgentStatus;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetDebugPlates;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetEnrichers;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetIgnores;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetWebhookForwards;
using Tests.TestHelpers;

namespace Tests.Controllers
{
    [TestFixture]
    public class SettingsControllerTests : TestBase
    {
        private SettingsController _controller;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _controller = new SettingsController(Mediator);
        }

        [Test]
        public async Task GetAgent_ReturnsCorrectResult()
        {
            // Arrange
            var expectedAgent = TestDataFactory.CreateTestAgentDto();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetAgentQuery>(), cancellationToken)
                .Returns(expectedAgent);

            // Act
            var result = await _controller.GetAgent(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Hostname.Should().Be(expectedAgent.Hostname);
            result.EndpointUrl.Should().Be(expectedAgent.EndpointUrl);
            result.IsDebugEnabled.Should().Be(expectedAgent.IsDebugEnabled);
        }

        [Test]
        public async Task GetAgent_CallsCorrectQuery()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetAgent(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Any<GetAgentQuery>(),
                cancellationToken);
        }

        [Test]
        public async Task GetAgentStatus_ReturnsCorrectResult()
        {
            // Arrange
            var expectedStatus = TestDataFactory.CreateTestAgentStatusDto();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetAgentStatusQuery>(), cancellationToken)
                .Returns(expectedStatus);

            // Act
            var result = await _controller.GetAgentStatus(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.IsConnected.Should().Be(expectedStatus.IsConnected);
            result.Hostname.Should().Be(expectedStatus.Hostname);
            result.Version.Should().Be(expectedStatus.Version);
            result.CpuCores.Should().Be(expectedStatus.CpuCores);
        }

        [Test]
        public async Task GetAgentStatus_CallsCorrectQuery()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetAgentStatus(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Any<GetAgentStatusQuery>(),
                cancellationToken);
        }

        [Test]
        public async Task DisableAgent_ValidId_ReturnsResult()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<DisableAgentCommand>(), cancellationToken)
                .Returns(true);

            // Act
            var result = await _controller.DisableAgent(agentId, cancellationToken);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task DisableAgent_CallsCorrectCommand()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.DisableAgent(agentId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<DisableAgentCommand>(cmd => cmd.AgentId == agentId),
                cancellationToken);
        }

        [Test]
        public async Task EnableAgent_ValidId_ReturnsResult()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<EnableAgentCommand>(), cancellationToken)
                .Returns(true);

            // Act
            var result = await _controller.EnableAgent(agentId, cancellationToken);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task EnableAgent_CallsCorrectCommand()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.EnableAgent(agentId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<EnableAgentCommand>(cmd => cmd.AgentId == agentId),
                cancellationToken);
        }

        [Test]
        public async Task UpsertAgent_ValidAgent_CallsCorrectCommand()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgentDto();
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.UpsertAgent(agent, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<UpsertAgentCommand>(cmd => cmd.Agent == agent),
                cancellationToken);
        }

        [Test]
        public async Task StartScrape_ReturnsAcceptedResult()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.StartScrape(cancellationToken);

            // Assert
            result.Should().BeOfType<StatusCodeResult>();
            var statusResult = result as StatusCodeResult;
            statusResult.StatusCode.Should().Be(202);
        }

        [Test]
        public async Task StartScrape_CallsCorrectCommand()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.StartScrape(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Any<AgentScrapeCommand>(),
                cancellationToken);
        }

        [Test]
        public async Task AddIgnore_ValidIgnore_CallsCorrectCommand()
        {
            // Arrange
            var ignore = TestDataFactory.CreateTestIgnoreDto("IGNORE123");
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.AddIgnore(ignore, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<AddIgnoreCommand>(cmd => cmd.Ignore == ignore),
                cancellationToken);
        }

        [Test]
        public async Task UpsertIgnore_ValidIgnores_CallsCorrectCommand()
        {
            // Arrange
            var ignores = new List<IgnoreDto>
            {
                TestDataFactory.CreateTestIgnoreDto("IGNORE1"),
                TestDataFactory.CreateTestIgnoreDto("IGNORE2")
            };
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.UpsertIgnore(ignores, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<UpsertIgnoresCommand>(cmd => cmd.Ignores == ignores),
                cancellationToken);
        }

        [Test]
        public async Task GetIgnores_ReturnsCorrectResult()
        {
            // Arrange
            var expectedIgnores = new List<IgnoreDto>
            {
                TestDataFactory.CreateTestIgnoreDto("IGNORE1"),
                TestDataFactory.CreateTestIgnoreDto("IGNORE2")
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetIgnoresQuery>(), cancellationToken)
                .Returns(expectedIgnores);

            // Act
            var result = await _controller.GetIgnores(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].PlateNumber.Should().Be("IGNORE1");
            result[1].PlateNumber.Should().Be("IGNORE2");
        }

        [Test]
        public async Task GetIgnores_CallsCorrectQuery()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetIgnores(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Any<GetIgnoresQuery>(),
                cancellationToken);
        }

        [Test]
        public async Task GetForwards_ReturnsCorrectResult()
        {
            // Arrange
            var expectedForwards = new List<WebhookForwardDto>
            {
                TestDataFactory.CreateTestWebhookForwardDto(),
                TestDataFactory.CreateTestWebhookForwardDto()
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetWebhookForwardsQuery>(), cancellationToken)
                .Returns(expectedForwards);

            // Act
            var result = await _controller.GetForwards(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].ForwardGroups.Should().BeTrue();
            result[1].ForwardGroups.Should().BeTrue();
        }

        [Test]
        public async Task GetForwards_CallsCorrectQuery()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetForwards(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Any<GetWebhookForwardsQuery>(),
                cancellationToken);
        }

        [Test]
        public async Task UpsertForwards_ValidForwards_CallsCorrectCommand()
        {
            // Arrange
            var forwards = new List<WebhookForwardDto>
            {
                TestDataFactory.CreateTestWebhookForwardDto(),
                TestDataFactory.CreateTestWebhookForwardDto()
            };
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.UpsertForwards(forwards, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<UpsertWebhookForwardsCommand>(cmd => cmd.WebhookForwards == forwards),
                cancellationToken);
        }

        [Test]
        public async Task GetEnrichers_ReturnsCorrectResult()
        {
            // Arrange
            var expectedEnricher = TestDataFactory.CreateTestEnricherDto();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetEnrichersQuery>(), cancellationToken)
                .Returns(expectedEnricher);

            // Act
            var result = await _controller.GetEnrichers(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.IsEnabled.Should().Be(expectedEnricher.IsEnabled);
            result.ApiKey.Should().Be(expectedEnricher.ApiKey);
            result.EnricherType.Should().Be(expectedEnricher.EnricherType);
        }

        [Test]
        public async Task GetEnrichers_CallsCorrectQuery()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetEnrichers(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Any<GetEnrichersQuery>(),
                cancellationToken);
        }

        [Test]
        public async Task UpsertEnrichers_ValidEnricher_CallsCorrectCommand()
        {
            // Arrange
            var enricher = TestDataFactory.CreateTestEnricherDto();
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.UpsertEnrichers(enricher, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<UpsertEnrichersCommand>(cmd => cmd.Enricher == enricher),
                cancellationToken);
        }

        [Test]
        public async Task TestEnrichers_ReturnsResult()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<TestEnrichersCommand>(), cancellationToken)
                .Returns(true);

            // Act
            var result = await _controller.TestEnrichers(cancellationToken);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task TestEnrichers_CallsCorrectCommand()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.TestEnrichers(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Any<TestEnrichersCommand>(),
                cancellationToken);
        }

        [Test]
        public async Task GetDebugPlates_ReturnsCorrectResult()
        {
            // Arrange
            var onlyFailedPlateGroups = true;
            var expectedResults = "{\"debug\": \"plates\"}";
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetDebugPlatesQuery>(), cancellationToken)
                .Returns(expectedResults);

            // Act
            var result = await _controller.GetDebugPlates(onlyFailedPlateGroups, cancellationToken);

            // Assert
            result.Should().BeOfType<ContentResult>();
            var contentResult = result as ContentResult;
            contentResult.Content.Should().Be(expectedResults);
            contentResult.ContentType.Should().Be("application/json");
        }

        [Test]
        public async Task GetDebugPlates_CallsCorrectQuery()
        {
            // Arrange
            var onlyFailedPlateGroups = true;
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetDebugPlates(onlyFailedPlateGroups, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetDebugPlatesQuery>(q => q.OnlyFailedPlateGroups == onlyFailedPlateGroups),
                cancellationToken);
        }

        [Test]
        public async Task DeleteDebugPlates_CallsCorrectCommand()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.DeleteDebugPlates(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Any<DeleteDebugPlatesCommand>(),
                cancellationToken);
        }
    }
} 