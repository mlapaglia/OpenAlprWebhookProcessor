using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebhook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprAgentScraper
{
    public class OpenAlprAgentScraper : IOpenAlprAgentScraper
    {
        private const long millisecondsToScrape = 86400000;

        private const string scrapeUrl = "/list?start={0}&end={1}";

        private const string metadataUrl = "/meta/{0}";

        private readonly IGroupWebhookHandler _groupWebhookHandler;

        private readonly HttpClient _httpClient;

        private readonly ILogger<OpenAlprAgentScraper> _logger;

        private readonly IImageRetrieverService _imageRetriever;

        public OpenAlprAgentScraper(
            IGroupWebhookHandler groupWebhookHandler,
            ILogger<OpenAlprAgentScraper> logger,
            IImageRetrieverService imageRetriever)
        {
            _groupWebhookHandler = groupWebhookHandler;
            _logger = logger;
            _httpClient = new HttpClient();
            _imageRetriever = imageRetriever;
        }

        public async Task<long> ScrapeAgentAsync(
            long lastSuccessfulScrapeEpoch,
            string agentEndpointUrl,
            CancellationToken cancellationToken)
        {
            if (lastSuccessfulScrapeEpoch == 0)
            {
                lastSuccessfulScrapeEpoch = await GetEarliestGroupEpochAsync(
                    agentEndpointUrl,
                    cancellationToken);
            }

            var startDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while (startDate > lastSuccessfulScrapeEpoch)
            {
                _logger.LogInformation("Scraping between {startTime} and {endTime}",
                    lastSuccessfulScrapeEpoch,
                    lastSuccessfulScrapeEpoch += millisecondsToScrape);

                var timer = new Stopwatch();
                timer.Start();

                var scrapeResults = await _httpClient.GetAsync(
                    agentEndpointUrl
                    + scrapeUrl
                        .Replace("{0}", lastSuccessfulScrapeEpoch.ToString())
                        .Replace("{1}", (lastSuccessfulScrapeEpoch + millisecondsToScrape).ToString()),
                    cancellationToken);

                timer.Stop();
                _logger.LogInformation("Scraping took {seconds} seconds", timer.Elapsed.Seconds);

                if (!scrapeResults.IsSuccessStatusCode)
                {
                    var error = await scrapeResults.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("no metadata found for given date range: {error}", error);
                    lastSuccessfulScrapeEpoch += millisecondsToScrape;
                    continue;
                }

                var content = await scrapeResults.Content.ReadAsStringAsync(cancellationToken);

                var metaDatasToQuery = JsonSerializer.Deserialize<List<ScrapeMetadata>>(content);

                _logger.LogInformation("Found {count} entries for: {date}",
                    metaDatasToQuery.Count,
                    lastSuccessfulScrapeEpoch.ToString());

                foreach (var metadata in metaDatasToQuery)
                {
                    _logger.LogDebug("querying key: {key}", metadata.Key);

                    timer.Reset();
                    timer.Start();

                    var newGroup = await _httpClient.GetAsync(
                        agentEndpointUrl + metadataUrl.Replace("{0}", metadata.Key),
                        cancellationToken);

                    timer.Stop();
                    _logger.LogDebug("Took {seconds} to query", timer.Elapsed.TotalSeconds);

                    if (!newGroup.IsSuccessStatusCode)
                    {
                        _logger.LogError("Bad response received from Agent: {statusCode} {reasonPhrase}", newGroup.StatusCode, newGroup.ReasonPhrase);
                        continue;
                    }

                    OpenAlprWebhook.Group group;

                    try
                    {
                        timer.Reset();
                        timer.Start();
                        _logger.LogDebug("deserializing key: {key}", metadata.Key);
                        group = await JsonSerializer.DeserializeAsync<OpenAlprWebhook.Group>(
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

                    lastSuccessfulScrapeEpoch = group.EpochStart;

                    timer.Stop();
                    _logger.LogDebug("Took {seconds} to update agent status.", timer.Elapsed.TotalSeconds);
                }

                lastSuccessfulScrapeEpoch += millisecondsToScrape;
            }

            if (lastSuccessfulScrapeEpoch > startDate)
            {
                lastSuccessfulScrapeEpoch = startDate;
            }

            _logger.LogInformation("Finished OpenALPR Agent scrape.");

            return lastSuccessfulScrapeEpoch;
        }

        public async Task ScrapeAgentImagesAsync(
            List<string> plateGroupIds,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Searching for plates with missings images");

            foreach (var plateGroupId in plateGroupIds)
            {
                _imageRetriever.TryAddJob(plateGroupId);
            }

            _logger.LogInformation("Jobs added successfully.");
        }

        private async Task<long> GetEarliestGroupEpochAsync(
            string agentEndpointUrl,
            CancellationToken cancellationToken)
        {
            var result = await _httpClient.GetAsync(
                agentEndpointUrl,
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
