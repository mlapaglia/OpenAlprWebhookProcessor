using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetVersion;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Queries.GetVersion
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetVersionQueryHandlerTests : TestBase
    {
        private GetVersionQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetVersionQueryHandler();
        }

        [Test]
        public async Task Handle_ValidQuery_ReturnsVersionDto()
        {
            // Arrange
            var query = new GetVersionQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().NotBeNullOrEmpty();
            result.AssemblyVersion.Should().NotBeNullOrEmpty();
            result.FileVersion.Should().NotBeNullOrEmpty();
            result.InformationalVersion.Should().NotBeNullOrEmpty();
        }

        [Test]
        public async Task Handle_ValidQuery_ReturnsCorrectVersionFromAssembly()
        {
            // Arrange
            var query = new GetVersionQuery();
            var cancellationToken = GetCancellationToken();
            var assembly = Assembly.GetExecutingAssembly();
            var expectedVersion = assembly.GetName().Version?.ToString() ?? "Unknown";

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Version.Should().Be(expectedVersion);
        }



        [Test]
        public async Task Handle_MultipleCallsWithSameQuery_ReturnsSameResult()
        {
            // Arrange
            var query = new GetVersionQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result1 = await _handler.Handle(query, cancellationToken);
            var result2 = await _handler.Handle(query, cancellationToken);

            // Assert
            result1.Version.Should().Be(result2.Version);
            result1.AssemblyVersion.Should().Be(result2.AssemblyVersion);
            result1.FileVersion.Should().Be(result2.FileVersion);
            result1.InformationalVersion.Should().Be(result2.InformationalVersion);
        }

        [Test]
        public void Handle_SynchronousCompletion_ReturnsCompletedValueTask()
        {
            // Arrange
            var query = new GetVersionQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var valueTask = _handler.Handle(query, cancellationToken);

            // Assert
            valueTask.IsCompleted.Should().BeTrue();
        }
    }
}
