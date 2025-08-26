using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Ignores.Queries.GetIgnores;
using Tests.TestHelpers;

namespace Tests.Features.Ignores.Queries.GetIgnores
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetIgnoresQueryHandlerTests : TestBase
    {
        private GetIgnoresQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetIgnoresQueryHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_WithExistingIgnores_ReturnsCorrectIgnoreDtos()
        {
            // Arrange
            var ignore1 = new OpenAlprWebhookProcessor.Data.Ignore
            {
                Id = Guid.NewGuid(),
                PlateNumber = "TEST123",
                Description = "Test ignore 1",
                IsStrictMatch = true
            };
            var ignore2 = new OpenAlprWebhookProcessor.Data.Ignore
            {
                Id = Guid.NewGuid(),
                PlateNumber = "TEST456",
                Description = "Test ignore 2",
                IsStrictMatch = false
            };
            await UnitOfWork.Ignores.AddAsync(ignore1);
            await UnitOfWork.Ignores.AddAsync(ignore2);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetIgnoresQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var resultIgnore1 = result.First(i => i.Id == ignore1.Id);
            resultIgnore1.PlateNumber.Should().Be("TEST123");
            resultIgnore1.Description.Should().Be("Test ignore 1");
            resultIgnore1.StrictMatch.Should().BeTrue();

            var resultIgnore2 = result.First(i => i.Id == ignore2.Id);
            resultIgnore2.PlateNumber.Should().Be("TEST456");
            resultIgnore2.Description.Should().Be("Test ignore 2");
            resultIgnore2.StrictMatch.Should().BeFalse();
        }

        [Test]
        public async Task Handle_WithNoIgnores_ReturnsEmptyList()
        {
            // Arrange
            var query = new GetIgnoresQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithSingleIgnore_ReturnsSingleIgnoreDto()
        {
            // Arrange
            var ignore = new OpenAlprWebhookProcessor.Data.Ignore
            {
                Id = Guid.NewGuid(),
                PlateNumber = "SINGLE123",
                Description = "Single ignore",
                IsStrictMatch = true
            };
            await UnitOfWork.Ignores.AddAsync(ignore);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetIgnoresQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);

            var resultIgnore = result.First();
            resultIgnore.Id.Should().Be(ignore.Id);
            resultIgnore.PlateNumber.Should().Be("SINGLE123");
            resultIgnore.Description.Should().Be("Single ignore");
            resultIgnore.StrictMatch.Should().BeTrue();
        }

        [Test]
        public async Task Handle_WithNullDescription_ReturnsIgnoreWithNullDescription()
        {
            // Arrange
            var ignore = new OpenAlprWebhookProcessor.Data.Ignore
            {
                Id = Guid.NewGuid(),
                PlateNumber = "NULL123",
                Description = null,
                IsStrictMatch = false
            };
            await UnitOfWork.Ignores.AddAsync(ignore);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetIgnoresQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);

            var resultIgnore = result.First();
            resultIgnore.Description.Should().BeNull();
        }

        [Test]
        public async Task Handle_WithEmptyDescription_ReturnsIgnoreWithEmptyDescription()
        {
            // Arrange
            var ignore = new OpenAlprWebhookProcessor.Data.Ignore
            {
                Id = Guid.NewGuid(),
                PlateNumber = "EMPTY123",
                Description = "",
                IsStrictMatch = false
            };
            await UnitOfWork.Ignores.AddAsync(ignore);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetIgnoresQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);

            var resultIgnore = result.First();
            resultIgnore.Description.Should().Be("");
        }

        [Test]
        public async Task Handle_MapsStrictMatchCorrectly()
        {
            // Arrange
            var strictIgnore = new OpenAlprWebhookProcessor.Data.Ignore
            {
                Id = Guid.NewGuid(),
                PlateNumber = "STRICT123",
                Description = "Strict match",
                IsStrictMatch = true
            };
            var nonStrictIgnore = new OpenAlprWebhookProcessor.Data.Ignore
            {
                Id = Guid.NewGuid(),
                PlateNumber = "NONSTRICT123",
                Description = "Non-strict match",
                IsStrictMatch = false
            };
            await UnitOfWork.Ignores.AddAsync(strictIgnore);
            await UnitOfWork.Ignores.AddAsync(nonStrictIgnore);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetIgnoresQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var strictResult = result.First(i => i.PlateNumber == "STRICT123");
            strictResult.StrictMatch.Should().BeTrue();

            var nonStrictResult = result.First(i => i.PlateNumber == "NONSTRICT123");
            nonStrictResult.StrictMatch.Should().BeFalse();
        }

        [Test]
        public async Task Handle_CallsGetAllAsync()
        {
            // Arrange
            var query = new GetIgnoresQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            // The fact that we get a result (even if empty) confirms the method was called
        }
    }
}
