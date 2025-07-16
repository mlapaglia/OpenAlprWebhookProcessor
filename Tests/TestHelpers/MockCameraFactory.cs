using NSubstitute;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Features.Cameras;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;
using System.IO;
using System.Threading;

namespace Tests.TestHelpers
{
    public class MockCameraFactory : ICameraFactory
    {
        private readonly ICamera _mockCamera;

        public MockCameraFactory()
        {
            _mockCamera = Substitute.For<ICamera>();
            
            // Configure the mock to return an empty stream by default
            _mockCamera.GetSnapshotAsync(Arg.Any<CancellationToken>())
                .Returns(new MemoryStream(TestDataFactory.CreateTestJpegBytes()));
        }

        public ICamera Create(CameraManufacturer cameraManufacturer, OpenAlprWebhookProcessor.Data.Camera camera)
        {
            return _mockCamera;
        }

        public ICamera MockCamera => _mockCamera;
    }
} 