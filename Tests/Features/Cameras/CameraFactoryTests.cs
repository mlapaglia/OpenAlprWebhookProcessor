using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Cameras;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.CameraUpdateService.Hikvision;
using OpenAlprWebhookProcessor.Features.Cameras;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;
using Tests.TestHelpers;

namespace Tests.Features.Cameras
{
    [TestFixture]
    public class CameraFactoryTests
    {
        private CameraFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new CameraFactory();
        }

        [Test]
        public void Create_HikvisionManufacturer_ReturnsHikvisionCamera()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);

            // Act
            var result = _factory.Create(CameraManufacturer.Hikvision, camera);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<HikvisionCamera>();
        }

        [Test]
        public void Create_DahuaManufacturer_ReturnsDahuaCamera()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Dahua);

            // Act
            var result = _factory.Create(CameraManufacturer.Dahua, camera);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<DahuaCamera>();
        }

        [Test]
        public void Create_InvalidManufacturer_ThrowsArgumentException()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);
            var invalidManufacturer = (CameraManufacturer)999; // Invalid enum value

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _factory.Create(invalidManufacturer, camera));
            
            exception.Message.Should().Be("unknown camera manufacturer");
        }

        [Test]
        public void Create_WithNullCamera_ShouldThrowNullReferenceException()
        {
            // Arrange
            OpenAlprWebhookProcessor.Data.Camera nullCamera = null;

            // Act & Assert
            // Both camera implementations will throw NullReferenceException when accessing null camera properties
            Assert.Throws<NullReferenceException>(() => _factory.Create(CameraManufacturer.Hikvision, nullCamera));
            Assert.Throws<NullReferenceException>(() => _factory.Create(CameraManufacturer.Dahua, nullCamera));
        }

        [Test]
        public void Create_HikvisionCamera_ImplementsICameraInterface()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);

            // Act
            var result = _factory.Create(CameraManufacturer.Hikvision, camera);

            // Assert
            result.Should().BeAssignableTo<ICamera>();
        }

        [Test]
        public void Create_DahuaCamera_ImplementsICameraInterface()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Dahua);

            // Act
            var result = _factory.Create(CameraManufacturer.Dahua, camera);

            // Assert
            result.Should().BeAssignableTo<ICamera>();
        }

        [Test]
        public void Create_AllSupportedManufacturers_ReturnsValidCameras()
        {
            // Arrange
            var supportedManufacturers = new[]
            {
                CameraManufacturer.Hikvision,
                CameraManufacturer.Dahua
            };

            // Act & Assert
            foreach (var manufacturer in supportedManufacturers)
            {
                var camera = TestDataFactory.CreateTestCamera(manufacturer);
                var result = _factory.Create(manufacturer, camera);
                result.Should().NotBeNull();
                result.Should().BeAssignableTo<ICamera>();
            }
        }

        [Test]
        public void Create_SameManufacturerMultipleTimes_ReturnsNewInstanceEachTime()
        {
            // Arrange
            var camera = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision);

            // Act
            var result1 = _factory.Create(CameraManufacturer.Hikvision, camera);
            var result2 = _factory.Create(CameraManufacturer.Hikvision, camera);

            // Assert
            result1.Should().NotBeSameAs(result2);
        }

        [Test]
        public void Create_DifferentCameraInstances_CreatesDistinctCameras()
        {
            // Arrange
            var camera1 = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision, Guid.NewGuid(), 1);
            var camera2 = TestDataFactory.CreateTestCamera(CameraManufacturer.Hikvision, Guid.NewGuid(), 2);

            // Act
            var result1 = _factory.Create(CameraManufacturer.Hikvision, camera1);
            var result2 = _factory.Create(CameraManufacturer.Hikvision, camera2);

            // Assert
            result1.Should().NotBeSameAs(result2);
            result1.Should().BeOfType<HikvisionCamera>();
            result2.Should().BeOfType<HikvisionCamera>();
        }
    }
} 