using AwesomeAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Alerts;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.TestPushover;
using Tests.TestHelpers;

namespace Tests.Features.Alerts.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class TestPushoverCommandHandlerTests : TestBase
    {
        private TestPushoverCommandHandler _handler;
        private MockPushoverClient _mockPushoverClient;
        private List<IAlertClient> _alertClients;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockPushoverClient = new MockPushoverClient();
            _alertClients = new List<IAlertClient> { _mockPushoverClient };
            
            _handler = new TestPushoverCommandHandler(UnitOfWork, _alertClients);
        }

        [Test]
        public async Task Handle_WithPlateGroupAndImage_SendsTestAlert()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroupWithImages(
                openAlprUuid: "test-uuid-123", 
                withPlateImage: true);
            
            Context.PlateGroups.Add(plateGroup);
            await Context.SaveChangesAsync();

            var command = new TestPushoverCommand();
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _mockPushoverClient.VerifyCredentialsCallCount.Should().Be(1);
            _mockPushoverClient.SendAlertCallCount.Should().Be(1);
            
            var sentAlert = _mockPushoverClient.LastSentAlert;
            sentAlert.Should().NotBeNull();
            sentAlert.PlateId.Should().Be(plateGroup.Id);
            sentAlert.PlateNumber.Should().Be(plateGroup.BestNumber);
            sentAlert.PlateJpeg.Should().BeEquivalentTo(plateGroup.PlateImage.Jpeg);
            sentAlert.PlateJpegUrl.Should().Be($"/api/images/crop/{plateGroup.OpenAlprUuid}");
            sentAlert.IsUrgent.Should().BeTrue();
            sentAlert.Description.Should().Contain("was seen on");
        }

        [Test]
        public async Task Handle_WithNoPlateGroupWithImage_ThrowsInvalidOperationException()
        {
            // Arrange
            var plateGroupWithoutImage = TestDataFactory.CreateTestPlateGroupWithImages(
                withPlateImage: false);
            
            Context.PlateGroups.Add(plateGroupWithoutImage);
            await Context.SaveChangesAsync();

            var command = new TestPushoverCommand();
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, cancellationToken));
            
            exception.Message.Should().Be("No test plate group with image found in the database");
        }

        [Test]
        public void Handle_WithEmptyDatabase_ThrowsInvalidOperationException()
        {
            // Arrange
            var command = new TestPushoverCommand();
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be("No test plate group with image found in the database");
        }

        [Test]
        public async Task Handle_VerifyCredentialsThrows_PropagatesException()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroupWithImages(withPlateImage: true);
            Context.PlateGroups.Add(plateGroup);
            await Context.SaveChangesAsync();

            var command = new TestPushoverCommand();
            var cancellationToken = GetCancellationToken();
            
            var credentialsException = new InvalidOperationException("Invalid credentials");
            _mockPushoverClient.SetVerifyCredentialsException(credentialsException);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, cancellationToken));
            
            exception.Should().Be(credentialsException);
        }

        [Test]
        public async Task Handle_SendAlertThrows_PropagatesException()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroupWithImages(withPlateImage: true);
            Context.PlateGroups.Add(plateGroup);
            await Context.SaveChangesAsync();

            var command = new TestPushoverCommand();
            var cancellationToken = GetCancellationToken();
            
            var sendAlertException = new InvalidOperationException("Send alert failed");
            _mockPushoverClient.SetSendAlertException(sendAlertException);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, cancellationToken));
            
            exception.Should().Be(sendAlertException);
        }

        [Test]
        public async Task Handle_FindsPushoverClientFromMultipleClients()
        {
            // Arrange
            var mockWebPushClient = Substitute.For<IAlertClient>();
            var mockPushoverClient = new MockPushoverClient();
            
            var alertClients = new List<IAlertClient> { mockWebPushClient, mockPushoverClient };
            var handler = new TestPushoverCommandHandler(UnitOfWork, alertClients);
            
            var plateGroup = TestDataFactory.CreateTestPlateGroupWithImages(withPlateImage: true);
            Context.PlateGroups.Add(plateGroup);
            await Context.SaveChangesAsync();

            var command = new TestPushoverCommand();
            var cancellationToken = GetCancellationToken();

            // Act
            await handler.Handle(command, cancellationToken);

            // Assert - Verify the PushoverClient was called and the other client was not
            mockPushoverClient.VerifyCredentialsCallCount.Should().Be(1);
            mockPushoverClient.SendAlertCallCount.Should().Be(1);
            
            await mockWebPushClient.DidNotReceive().VerifyCredentialsAsync(Arg.Any<CancellationToken>());
            await mockWebPushClient.DidNotReceive().SendAlertAsync(Arg.Any<AlertUpdateRequest>(), Arg.Any<CancellationToken>());
        }
    }

    public class MockPushoverClient : PushoverClient, IAlertClient
    {
        public int VerifyCredentialsCallCount { get; private set; }
        public int SendAlertCallCount { get; private set; }
        public AlertUpdateRequest LastSentAlert { get; private set; }
        
        private Exception _verifyCredentialsException;
        private Exception _sendAlertException;

        public MockPushoverClient() : base(null, null)
        {
        }

        async Task IAlertClient.VerifyCredentialsAsync(CancellationToken cancellationToken)
        {
            VerifyCredentialsCallCount++;
            if (_verifyCredentialsException != null)
                throw _verifyCredentialsException;
            await Task.CompletedTask;
        }

        async Task IAlertClient.SendAlertAsync(AlertUpdateRequest alert, CancellationToken cancellationToken)
        {
            SendAlertCallCount++;
            LastSentAlert = alert;
            if (_sendAlertException != null)
                throw _sendAlertException;
            await Task.CompletedTask;
        }

        async Task<bool> IAlertClient.ShouldSendAllPlatesAsync(CancellationToken cancellationToken)
        {
            return await Task.FromResult(false);
        }

        public void SetVerifyCredentialsException(Exception exception)
        {
            _verifyCredentialsException = exception;
        }

        public void SetSendAlertException(Exception exception)
        {
            _sendAlertException = exception;
        }
    }
}
