using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlate;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Queries.GetPlate
{
    [TestFixture]
    public class GetPlateQueryHandlerTests : TestBase
    {
        private GetPlateQueryHandler _handler;
        private IUnitOfWork _unitOfWork;
        private IPlateGroupRepository _plateGroupRepository;
        private IRepository<Ignore> _ignoreRepository;
        private IRepository<Alert> _alertRepository;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _plateGroupRepository = Substitute.For<IPlateGroupRepository>();
            _ignoreRepository = Substitute.For<IRepository<Ignore>>();
            _alertRepository = Substitute.For<IRepository<Alert>>();
            
            _unitOfWork.PlateGroups.Returns(_plateGroupRepository);
            _unitOfWork.Ignores.Returns(_ignoreRepository);
            _unitOfWork.Alerts.Returns(_alertRepository);
            
            _handler = new GetPlateQueryHandler(_unitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            _unitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ValidPlateId_ReturnsPlateWithMappedData()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var query = new GetPlateQuery(plateId);
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;
            plateGroup.BestNumber = "ABC123";

            var ignores = new List<Ignore>
            {
                new Ignore { PlateNumber = "IGNORE1" },
                new Ignore { PlateNumber = "IGNORE2" }
            };

            var alerts = new List<Alert>
            {
                new Alert { PlateNumber = "ALERT1" },
                new Alert { PlateNumber = "ALERT2" }
            };

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            _ignoreRepository.GetAllAsync(cancellationToken)
                .Returns(ignores);

            _alertRepository.GetAllAsync(cancellationToken)
                .Returns(alerts);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(plateId);
            result.PlateNumber.Should().Be("ABC123");
            
            // Verify that PlateMapper was called with correct parameters
            await _plateGroupRepository.Received(1).GetByIdWithDetailsAsync(plateId, cancellationToken);
            await _ignoreRepository.Received(1).GetAllAsync(cancellationToken);
            await _alertRepository.Received(1).GetAllAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_PlateNotFound_ReturnsNull()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var query = new GetPlateQuery(plateId);
            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, cancellationToken)
                .Returns((PlateGroup)null);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeNull();
            
            // Verify that ignore and alert repositories were not called
            await _ignoreRepository.DidNotReceive().GetAllAsync(cancellationToken);
            await _alertRepository.DidNotReceive().GetAllAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_EmptyGuid_ReturnsNull()
        {
            // Arrange
            var query = new GetPlateQuery(Guid.Empty);
            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdWithDetailsAsync(Guid.Empty, cancellationToken)
                .Returns((PlateGroup)null);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeNull();
            
            await _ignoreRepository.DidNotReceive().GetAllAsync(cancellationToken);
            await _alertRepository.DidNotReceive().GetAllAsync(cancellationToken);
        }

        [Test]
        public void Handle_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var query = new GetPlateQuery(plateId);
            var cancellationToken = GetCancellationToken();
            var repositoryException = new InvalidOperationException("Database connection failed");

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, cancellationToken)
                .ThrowsAsync(repositoryException);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(query, cancellationToken));

            exception.Should().Be(repositoryException);
        }

        [Test]
        public void Handle_IgnoreRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var query = new GetPlateQuery(plateId);
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;

            var cancellationToken = GetCancellationToken();
            var ignoreException = new InvalidOperationException("Ignore repository failed");

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            _ignoreRepository.GetAllAsync(cancellationToken)
                .ThrowsAsync(ignoreException);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(query, cancellationToken));

            exception.Should().Be(ignoreException);
        }

        [Test]
        public void Handle_AlertRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var query = new GetPlateQuery(plateId);
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;

            var ignores = new List<Ignore>
            {
                new Ignore { PlateNumber = "IGNORE1" }
            };

            var cancellationToken = GetCancellationToken();
            var alertException = new InvalidOperationException("Alert repository failed");

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            _ignoreRepository.GetAllAsync(cancellationToken)
                .Returns(ignores);

            _alertRepository.GetAllAsync(cancellationToken)
                .ThrowsAsync(alertException);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(query, cancellationToken));

            exception.Should().Be(alertException);
        }

        [Test]
        public async Task Handle_EmptyIgnoreList_StillReturnsPlate()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var query = new GetPlateQuery(plateId);
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;
            plateGroup.BestNumber = "ABC123";

            var emptyIgnores = new List<Ignore>();
            var alerts = new List<Alert>
            {
                new Alert { PlateNumber = "ALERT1" }
            };

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            _ignoreRepository.GetAllAsync(cancellationToken)
                .Returns(emptyIgnores);

            _alertRepository.GetAllAsync(cancellationToken)
                .Returns(alerts);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(plateId);
            result.PlateNumber.Should().Be("ABC123");
        }

        [Test]
        public async Task Handle_EmptyAlertList_StillReturnsPlate()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var query = new GetPlateQuery(plateId);
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;
            plateGroup.BestNumber = "ABC123";

            var ignores = new List<Ignore>
            {
                new Ignore { PlateNumber = "IGNORE1" }
            };
            var emptyAlerts = new List<Alert>();

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            _ignoreRepository.GetAllAsync(cancellationToken)
                .Returns(ignores);

            _alertRepository.GetAllAsync(cancellationToken)
                .Returns(emptyAlerts);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(plateId);
            result.PlateNumber.Should().Be("ABC123");
        }
    }
} 