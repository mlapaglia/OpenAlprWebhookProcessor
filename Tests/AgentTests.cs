using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Settings;

namespace Tests
{
    public class AgentTests
    {
        private EfContextCreator _contextCreator;

        [SetUp]
        public void Setup()
        {
            _contextCreator = new EfContextCreator();
        }

        [Test]
        public async Task AgentRequestSucceeds()
        {
            using (var processorContext = _contextCreator.CreateContext())
            {
                if (processorContext.Database.EnsureCreated())
                {
                    processorContext.Agents.Add(new OpenAlprWebhookProcessor.Data.Agent()
                    {
                        EndpointUrl = "http://google.com",
                        Hostname = "localhost",
                        Id = Guid.NewGuid(),
                        IsDebugEnabled = true,
                        IsImageCompressionEnabled = true,
                        LastHeartbeatEpochMs = 0,
                        LastSuccessfulScrapeEpoch = 0,
                        Latitude = 0,
                        Longitude = 0,
                        NextScrapeEpochMs = 0,
                        OpenAlprWebServerApiKey = null,
                        OpenAlprWebServerUrl = null,
                        ScheduledScrapingIntervalMinutes = 0,
                        SunriseOffset = 0,
                        SunsetOffset = 0,
                        TimeZoneOffset = 0,
                        Uid = null,
                        Version = null,
                    });

                    await processorContext.SaveChangesAsync();
                    var handler = new GetAgentRequestHandler(processorContext);

                    var agentResult = await handler.HandleAsync(default);

                    Assert.That(agentResult, Is.Not.Null);
                }
            }
        }
    }
}