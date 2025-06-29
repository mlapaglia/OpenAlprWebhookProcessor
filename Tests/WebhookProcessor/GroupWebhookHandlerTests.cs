using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenAlprWebhookProcessor.Server.Alerts;
using OpenAlprWebhookProcessor.Server.CameraUpdateService;
using OpenAlprWebhookProcessor.Server.Data;
using OpenAlprWebhookProcessor.Server.ProcessorHub;
using OpenAlprWebhookProcessor.Server.WebhookProcessor;

namespace Tests.WebhookProcessor
{
    public class GroupWebhookHandlerTests
    {
        private readonly ILogger<GroupWebhookHandler> _logger;

        private readonly ICameraUpdateService _cameraUpdateService;

        private readonly ProcessorContextCreator _processorContextCreator;

        private readonly IHubContext<ProcessorHub, IProcessorHub> _processorHub;

        private readonly IAlertService _alertService;

        private readonly IImageRetrieverService _imageRetrieverService;

        public GroupWebhookHandlerTests()
        {
            _logger = Substitute.For<ILogger<GroupWebhookHandler>>();
            _cameraUpdateService = Substitute.For<ICameraUpdateService>();
            _processorContextCreator = new ProcessorContextCreator();
            _processorHub = Substitute.For<IHubContext<ProcessorHub, IProcessorHub>>();
            _alertService = Substitute.For<IAlertService>();
            _imageRetrieverService = Substitute.For<IImageRetrieverService>();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _processorContextCreator.Dispose();
        }

        [Test]
        public void GroupWebhookHandler()
        {
            var groupWebhookHandler = new GroupWebhookHandler(
                _logger,
                _cameraUpdateService,
                _processorContextCreator.CreateContext(),
                _processorHub,
                _alertService,
                _imageRetrieverService);

            Assert.That(groupWebhookHandler, Is.Not.Null);
        }

        [Test]
        public async Task DebugPlatesShouldSave()
        {
            using (var processorContext = _processorContextCreator.CreateContext())
            {
                processorContext.Agents.Add(new Agent()
                {
                    IsDebugEnabled = true,
                });

                await processorContext.SaveChangesAsync();

                var handler = CreateHandler();

                await handler.HandleWebhookAsync(
                    WebhookFaker.Generate().First(),
                    false,
                    CancellationToken.None);
            }

            using (var processorContext = _processorContextCreator.CreateContext())
            {
                var rawPlateGroups = await processorContext.RawPlateGroups.ToListAsync();
                Assert.That(rawPlateGroups, Has.Exactly(1).Items);
            }
        }

        [Test]
        public async Task AgentMissingShouldNotSave()
        {
            using (var processorContext = _processorContextCreator.CreateContext())
            {
                var handler = CreateHandler();

                await handler.HandleWebhookAsync(
                    WebhookFaker.Generate().First(),
                    false,
                    CancellationToken.None);
            }

            _logger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        public GroupWebhookHandler CreateHandler()
        {
            return new GroupWebhookHandler(
                _logger,
                _cameraUpdateService,
                _processorContextCreator.CreateContext(),
                _processorHub,
                _alertService,
                _imageRetrieverService);
        }
    }
}
