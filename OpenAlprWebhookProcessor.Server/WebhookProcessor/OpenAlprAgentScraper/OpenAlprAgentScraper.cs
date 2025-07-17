using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprAgentScraper
{
    public class OpenAlprAgentScraper : IOpenAlprAgentScraper
    {
        private const long millisecondsToScrape = 86400000;

        private const string scrapeUrl = "/list?start={0}&end={1}";

        private const string metadataUrl = "/meta/{0}";

        private readonly IGroupWebhookHandler _groupWebhookHandler;

        private readonly HttpClient _httpClient;

        private readonly ProcessorContext _processorContext;

        private readonly ILogger<OpenAlprAgentScraper> _logger;

        private readonly IImageRetrieverService _imageRetriever;

        private readonly ITimeService _timeService;

        public OpenAlprAgentScraper(
            IGroupWebhookHandler groupWebhookHandler,
            ProcessorContext processorContext,
            ILogger<OpenAlprAgentScraper> logger,
            IImageRetrieverService imageRetriever,
            HttpClient httpClient,
            ITimeService timeService)
        {
            _groupWebhookHandler = groupWebhookHandler;
            _processorContext = processorContext;
            _logger = logger;
            _httpClient = httpClient;
            _imageRetriever = imageRetriever;
            _timeService = timeService;
        }

        public async Task ScrapeAgentAsync(CancellationToken cancellationToken)
        {
            var agent = await _processorContext.Agents.FirstOrDefaultAsync(cancellationToken);

            if (agent.LastSuccessfulScrapeEpoch == 0)
            {
                agent.LastSuccessfulScrapeEpoch = await GetEarliestGroupEpochAsync(
                    agent,
                    cancellationToken);
            }

            var startDate = _timeService.UtcNowMilliseconds;
            while (startDate > agent.LastSuccessfulScrapeEpoch)
            {
                await ScrapeDataForTimeRange(agent, cancellationToken);
                agent.LastSuccessfulScrapeEpoch += millisecondsToScrape;
            }

            if (agent.LastSuccessfulScrapeEpoch > startDate)
            {
                agent.LastSuccessfulScrapeEpoch = startDate;
            }

            await _processorContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Finished OpenALPR Agent scrape.");
        }

        private async Task ScrapeDataForTimeRange(Agent agent, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Scraping between {StartTime} and {EndTime}",
                agent.LastSuccessfulScrapeEpoch,
                agent.LastSuccessfulScrapeEpoch += millisecondsToScrape);

            var timer = new Stopwatch();
            timer.Start();

            var scrapeResults = await _httpClient.GetAsync(
                BuildScrapeUrl(agent.EndpointUrl, agent.LastSuccessfulScrapeEpoch, agent.LastSuccessfulScrapeEpoch + millisecondsToScrape),
                cancellationToken);

            timer.Stop();
            _logger.LogInformation("Scraping took {Seconds} seconds", timer.Elapsed.Seconds);

            if (!scrapeResults.IsSuccessStatusCode)
            {
                var error = await scrapeResults.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("no metadata found for given date range: {Error}", error);
                agent.LastSuccessfulScrapeEpoch = agent.LastSuccessfulScrapeEpoch += millisecondsToScrape;
                return;
            }

            var content = await scrapeResults.Content.ReadAsStringAsync(cancellationToken);
            var metaDatasToQuery = JsonSerializer.Deserialize<List<ScrapeMetadata>>(content);

            _logger.LogInformation("Found {Count} entries for: {Date}",
                metaDatasToQuery.Count,
                agent.LastSuccessfulScrapeEpoch.ToString());

            foreach (var metadata in metaDatasToQuery)
            {
                await ProcessMetadata(agent, metadata, cancellationToken);
            }
        }

        private async Task ProcessMetadata(Agent agent, ScrapeMetadata metadata, CancellationToken cancellationToken)
        {
            _logger.LogDebug("querying key: {Key}", metadata.Key);

            var timer = new Stopwatch();
            timer.Start();

            var newGroup = await _httpClient.GetAsync(
                BuildMetadataUrl(agent.EndpointUrl, metadata.Key),
                cancellationToken);

            timer.Stop();
            _logger.LogDebug("Took {Seconds} to query", timer.Elapsed.TotalSeconds);

            if (!newGroup.IsSuccessStatusCode)
            {
                _logger.LogError("Bad response received from Agent: {StatusCode} {ReasonPhrase}", newGroup.StatusCode, newGroup.ReasonPhrase);
                return;
            }

            Group group;
            try
            {
                timer.Reset();
                timer.Start();
                _logger.LogDebug("deserializing key: {Key}", metadata.Key);
                group = await JsonSerializer.DeserializeAsync<Group>(
                    await newGroup.Content.ReadAsStreamAsync(cancellationToken),
                    cancellationToken: cancellationToken);
                timer.Stop();
                _logger.LogDebug("Took {Seconds} to deserialize.", timer.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to deserialize response from Agent for meta id: {MetadataKey}", metadata.Key);
                return;
            }

            await ProcessGroup(agent, group, metadata.Key, cancellationToken);
        }

        private async Task ProcessGroup(Agent agent, Group group, string metadataKey, CancellationToken cancellationToken)
        {
            var timer = new Stopwatch();

            try
            {
                _logger.LogInformation("date: {Date} querying: {Key}", DateTimeOffset.FromUnixTimeMilliseconds(group.EpochStart).ToString(), metadataKey);

                timer.Start();
                await _groupWebhookHandler.HandleWebhookAsync(
                    new Webhook
                    {
                        Group = group,
                    },
                    true,
                    cancellationToken);
                timer.Stop();
                _logger.LogDebug("Took {Seconds} to process.", timer.Elapsed.TotalSeconds);
            }
            catch
            {
                _logger.LogError("Failed to parse bulk import request.");
            }

            timer.Reset();
            timer.Start();
            _logger.LogDebug("Saving agent status, last scrape {ScrapeEpoch}", group.EpochStart);

            agent.LastSuccessfulScrapeEpoch = group.EpochStart;
            await _processorContext.SaveChangesAsync(cancellationToken);
            timer.Stop();
            _logger.LogDebug("Took {Seconds} to update agent status.", timer.Elapsed.TotalSeconds);
        }

        private string BuildScrapeUrl(string endpointUrl, long startEpoch, long endEpoch)
        {
            return endpointUrl + scrapeUrl
                .Replace("{0}", startEpoch.ToString())
                .Replace("{1}", endEpoch.ToString());
        }

        private string BuildMetadataUrl(string endpointUrl, string key)
        {
            return endpointUrl + metadataUrl.Replace("{0}", key);
        }

        public async Task ScrapeAgentImagesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Searching for plates with missings images");

            var plateGroupIds = await _processorContext.PlateGroups
                .AsNoTracking()
                .Where(x => x.AgentImageScrapeOccurredOn == null)
                .Select(x => x.OpenAlprUuid)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} plates to query the Agent for.", plateGroupIds.Count);

            foreach (var plateGroupId in plateGroupIds)
            {
                _imageRetriever.AddImageRetrievalJob(plateGroupId);
            }

            _logger.LogInformation("Jobs added successfully.");
        }

        private async Task<long> GetEarliestGroupEpochAsync(
            Agent agent,
            CancellationToken cancellationToken)
        {
            var result = await _httpClient.GetAsync(
                agent.EndpointUrl,
                cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                throw new ArgumentException($"Unable to get earliest group epoch: Status code: {result.StatusCode} Reason: {result.ReasonPhrase}");
            }

            var content = await result.Content.ReadAsStringAsync(cancellationToken);

            var firstEpochMs = Regex.Match(content, "Earliest date epoch: ([0-9]+)").Groups[1].Value;

            return long.Parse(firstEpochMs);
        }
    }
}
