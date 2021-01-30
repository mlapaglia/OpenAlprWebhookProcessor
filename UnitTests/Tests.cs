using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAlprWebhookProcessor.AgentImageRelay.GetImage;
using OpenAlprWebhookProcessor.AlertService;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.ProcessorHub;
using OpenAlprWebhookProcessor.Settings.GetCameras;
using OpenAlprWebhookProcessor.Settings.UpdatedCameras;
using OpenAlprWebhookProcessor.WebhookProcessor;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Unit
{
    public class AgentRelayHandlerTests
    {
        [Fact]
        public async Task GenerateCorrectUrl()
        {
            var processorOptions = new DbContextOptionsBuilder<ProcessorContext>()
                .UseSqlite("Filename=Test.db")
                .Options;

            using (var processorContext = new ProcessorContext(processorOptions))
            {
                var serviceProvider = new ServiceCollection()
                    .AddSingleton(processorContext)
                    .BuildServiceProvider();

                await processorContext.Database.MigrateAsync();
                var agenthandler = new UpsertAgentRequestHandler(processorContext);

                var rawWebhook = File.ReadAllText("./TestWebhook.json");
                var webhook = new Webhook
                {
                    Group = JsonSerializer.Deserialize<Group>(rawWebhook),
                };

                await agenthandler.HandleAsync(new OpenAlprWebhookProcessor.Settings.Agent()
                {
                    EndpointUrl = "http://test.com",
                    Hostname = "abc123",
                    OpenAlprWebServerApiKey = "23904jasdfoijh;3",
                    OpenAlprWebServerUrl = "http://alpr.test.com",
                    Uid = "23490-23-048209580345",
                    Version = "2.8.101",
                });

                var cameraHandler = new UpsertCameraHandler(processorContext);

                await cameraHandler.UpsertCameraAsync(new OpenAlprWebhookProcessor.Settings.Camera()
                {
                    CameraPassword = "asdf",
                    CameraUsername = "asdf",
                    Id = Guid.NewGuid(),
                    Latitude = 101,
                    Longitude = 101,
                    Manufacturer = OpenAlprWebhookProcessor.Cameras.Configuration.CameraManufacturer.Dahua,
                    ModelNumber = "asdf",
                    OpenAlprCameraId = webhook.Group.CameraId,
                    OpenAlprName = "asdf",
                    PlatesSeen = 123,
                    SampleImageUrl = "http://google.com/image.png",
                    UpdateOverlayTextUrl = "http://192.168.1.101/update"
                });

                var mockedLogger = new Mock<ILogger<CameraUpdateService>>();
                var cameraUpdateService = new CameraUpdateService(serviceProvider, mockedLogger.Object);

                var mockedWebhookLogger = new Mock<ILogger<WebhookHandler>>();
                var mockedHub = new Mock<IHubContext<ProcessorHub, IProcessorHub>>();
                var mockedCameraLogger = new Mock<ILogger<AlertService>>();
                var alertServices = new AlertService(serviceProvider, mockedCameraLogger.Object, mockedHub.Object);
                var handler = new WebhookHandler(mockedWebhookLogger.Object, cameraUpdateService, processorContext, mockedHub.Object, alertServices);

                await handler.HandleWebhookAsync(webhook);

                Assert.True(true);
            }
        }
    }
}
