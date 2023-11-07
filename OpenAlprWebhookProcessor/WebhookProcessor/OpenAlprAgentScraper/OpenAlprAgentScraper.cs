using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.ImageRelay;
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
    public class OpenAlprAgentScraper
    {
        private const int minutesToScrape = 60 * 24;

        private const string scrapeUrl = "/list?start={0}&end={1}";

        private const string metadataUrl = "/meta/{0}";

        private readonly GroupWebhookHandler _groupWebhookHandler;

        private readonly HttpClient _httpClient;

        private readonly ProcessorContext _processorContext;

        private readonly ILogger<OpenAlprAgentScraper> _logger;

        private readonly ImageRetrieverService _imageRetriever;

        public OpenAlprAgentScraper(
            GroupWebhookHandler groupWebhookHandler,
            ProcessorContext processorContext,
            ILogger<OpenAlprAgentScraper> logger,
            ImageRetrieverService imageRetriever)
        {
            _groupWebhookHandler = groupWebhookHandler;
            _processorContext = processorContext;
            _logger = logger;
            _httpClient = new HttpClient();
            _imageRetriever = imageRetriever;
        }

        public async Task ScrapeAgentAsync(CancellationToken cancellationToken)
        {
            var agent = await _processorContext.Agents.FirstOrDefaultAsync(cancellationToken);

            DateTimeOffset lastSuccessfulScrape;
            if (agent.LastSuccessfulScrapeEpoch == 0)
            {
                lastSuccessfulScrape = await GetEarliestGroupEpochAsync(
                    agent,
                    cancellationToken);
            }
            else
            {
                lastSuccessfulScrape = DateTimeOffset.FromUnixTimeMilliseconds(agent.LastSuccessfulScrapeEpoch);
            }

            var startDate = DateTimeOffset.UtcNow;
            while (startDate > lastSuccessfulScrape)
            {
                _logger.LogInformation("Scraping between {startTime} and {endTime}",
                    lastSuccessfulScrape.ToUnixTimeMilliseconds().ToString(),
                    lastSuccessfulScrape.AddMinutes(minutesToScrape).ToUnixTimeMilliseconds().ToString());

                var timer = new Stopwatch();
                timer.Start();

                var scrapeResults = await _httpClient.GetAsync(
                    agent.EndpointUrl
                    + scrapeUrl
                        .Replace("{0}", lastSuccessfulScrape.ToUnixTimeMilliseconds().ToString())
                        .Replace("{1}", lastSuccessfulScrape.AddMinutes(minutesToScrape).ToUnixTimeMilliseconds().ToString()),
                    cancellationToken);

                timer.Stop();
                _logger.LogInformation("Scraping took {seconds} seconds", timer.Elapsed.Seconds);

                if (!scrapeResults.IsSuccessStatusCode)
                {
                    var error = await scrapeResults.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("no metadata found for given date range: {error}", error);
                    lastSuccessfulScrape = lastSuccessfulScrape.AddMinutes(minutesToScrape);
                    continue;
                }

                var content = await scrapeResults.Content.ReadAsStringAsync(cancellationToken);

                var metaDatasToQuery = JsonSerializer.Deserialize<List<ScrapeMetadata>>(content);

                _logger.LogInformation("Found {count} entries for: {date}",
                    metaDatasToQuery.Count,
                    lastSuccessfulScrape.ToString());

                foreach (var metadata in metaDatasToQuery)
                {
                    _logger.LogDebug("querying key: {key}", metadata.Key);

                    timer.Reset();
                    timer.Start();

                    var newGroup = await _httpClient.GetAsync(
                        agent.EndpointUrl + metadataUrl.Replace("{0}", metadata.Key),
                        cancellationToken);

                    timer.Stop();
                    _logger.LogDebug("Took {seconds} to query", timer.Elapsed.TotalSeconds);

                    if (!newGroup.IsSuccessStatusCode)
                    {
                        _logger.LogError("Bad response received from Agent: {statusCode} {reasonPhrase}", newGroup.StatusCode, newGroup.ReasonPhrase);
                        continue;
                    }

                    Group group;

                    try
                    {
                        timer.Reset();
                        timer.Start();
                        _logger.LogDebug("deserializing key: {key}", metadata.Key);
                        group = await JsonSerializer.DeserializeAsync<Group>(
                            await newGroup.Content.ReadAsStreamAsync(cancellationToken),
                            cancellationToken: cancellationToken);
                        timer.Stop();
                        _logger.LogDebug("Took {seconds} to deserialize.", timer.Elapsed.TotalSeconds);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unable to deserialize response from Agent for meta id: {metadatakey}", metadata.Key);
                        continue;
                    }

                    try
                    {
                        _logger.LogInformation("date: {date} querying: {key}", DateTimeOffset.FromUnixTimeMilliseconds(group.EpochStart).ToString(), metadata.Key);

                        timer.Reset();
                        timer.Start();
                        await _groupWebhookHandler.HandleWebhookAsync(
                            new Webhook
                            {
                                Group = group,
                            },
                            true,
                            cancellationToken);
                        timer.Stop();
                        _logger.LogDebug("Took {seconds} to process.", timer.Elapsed.TotalSeconds);
                    }
                    catch
                    {
                        _logger.LogError("Failed to parse bulk import request.");
                    }

                    timer.Reset();
                    timer.Start();
                    _logger.LogDebug("Saving agent status, last scrape {scrapeEpoch}", group.EpochStart);

                    agent.LastSuccessfulScrapeEpoch = group.EpochStart;
                    await _processorContext.SaveChangesAsync(cancellationToken);
                    timer.Stop();
                    _logger.LogDebug("Took {seconds} to update agent status.", timer.Elapsed.TotalSeconds);
                }

                lastSuccessfulScrape = lastSuccessfulScrape.AddMinutes(minutesToScrape);

                if (lastSuccessfulScrape > startDate)
                {
                    lastSuccessfulScrape = startDate;
                }
            }

            _logger.LogInformation("Finished OpenALPR Agent scrape.");
        }

        public async Task ScrapeAgentImagesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Searching for plates with missings images");

            var plateGroupIds = await _processorContext.PlateGroups
                .AsNoTracking()
                .Where(x => x.AgentImageScrapeOccurredOn == null)
                .Select(x => x.OpenAlprUuid)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {count} plates to query the Agent for.", plateGroupIds.Count);

            foreach (var plateGroupId in plateGroupIds)
            {
                _imageRetriever.AddJob(plateGroupId);
            }

            _logger.LogInformation("Jobs added successfully.");
        }

        private async Task<DateTimeOffset> GetEarliestGroupEpochAsync(
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

            return DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(firstEpochMs));
        }
    }
}
