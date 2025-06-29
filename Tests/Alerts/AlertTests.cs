using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenAlprWebhookProcessor.Server.Alerts;
using OpenAlprWebhookProcessor.Server.ProcessorHub;

namespace Tests.Alerts
{
    [TestFixture]
    public class AlertServiceTests
    {
        private ILogger<AlertService> _mockLogger;
        private IHubContext<ProcessorHub, IProcessorHub> _mockHubContext;
        private IProcessorHub _mockProcessorHub;
        private List<IAlertClient> _mockAlertClients;
        private AlertService _alertService;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = Substitute.For<ILogger<AlertService>>();
            _mockProcessorHub = Substitute.For<IProcessorHub>();
            _mockProcessorHub.LicensePlateAlerted(Arg.Any<string>())
                .Returns(Task.CompletedTask);

            var clients = Substitute.For<IHubClients<IProcessorHub>>();
            clients.All.Returns(_mockProcessorHub);

            _mockHubContext = Substitute.For<IHubContext<ProcessorHub, IProcessorHub>>();
            _mockHubContext.Clients.Returns(clients);

            _mockAlertClients = new List<IAlertClient>();

            _alertService = new AlertService(
                _mockLogger,
                _mockHubContext,
                _mockAlertClients);
        }

        [Test]
        public void AddJob_ShouldQueueRequest()
        {
            var request = new AlertUpdateRequest
            {
                PlateNumber = "ABC123"
            };

            Assert.DoesNotThrow(() => _alertService.AddJob(request));
        }

        [Test]
        public async Task ProcessAlertsAsync_ShouldAlertAllClients_AndBroadcast()
        {
            var request = new AlertUpdateRequest
            {
                PlateNumber = "XYZ789"
            };

            var alertClient = Substitute.For<IAlertClient>();

            alertClient.SendAlertAsync(
                request,
                Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

            _mockAlertClients.Add(alertClient);

            var testService = new AlertService(
                _mockLogger,
                _mockHubContext,
                _mockAlertClients);

            testService.AddJob(request);

            await testService.StartAsync(CancellationToken.None);
            await Task.Delay(100); // let it process the alert
            await testService.StopAsync(CancellationToken.None);

            await alertClient.Received(1).SendAlertAsync(request, Arg.Any<CancellationToken>());
            await _mockProcessorHub.Received(1).LicensePlateAlerted("XYZ789");
        }

        [Test]
        public async Task ProcessAlertsAsync_ShouldLogError_WhenClientThrows()
        {
            var request = new AlertUpdateRequest { PlateNumber = "ERR500" };
            var failingClient = Substitute.For<IAlertClient>();

            failingClient.SendAlertAsync(request, Arg.Any<CancellationToken>())
                .Returns(_ => throw new Exception("Send failed"));

            _mockAlertClients.Add(failingClient);
            var testService = new AlertService(_mockLogger, _mockHubContext, _mockAlertClients);

            testService.AddJob(request);

            await testService.StartAsync(CancellationToken.None);
            await Task.Delay(100); // let it process the alert
            await testService.StopAsync(CancellationToken.None);

            await failingClient.Received(1).SendAlertAsync(request, Arg.Any<CancellationToken>());
        }
    }
}
