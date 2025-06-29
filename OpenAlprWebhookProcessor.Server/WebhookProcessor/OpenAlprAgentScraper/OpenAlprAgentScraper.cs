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

        /// <summary>
        /// Find all plate groups present on the Agent that have
        /// not been scraped already.
        /// </summary>
        /// <param name="lastSuccessfulScrapeEpoch">The date to start scraping from.</param>
        /// <param name="agentEndpointUrl">The URL to reach the Agent.</param>
        /// <param name="cancellationToken">Cancels the call to the Agent.</param>
        /// <returns></returns>
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
                _logger.LogInformation("scraping between {StartTime} and {EndTime}",
                    lastSuccessfulScrapeEpoch,
                    lastSuccessfulScrapeEpoch + millisecondsToScrape);

                lastSuccessfulScrapeEpoch += millisecondsToScrape;

                var timer = new Stopwatch();
                timer.Start();

                var scrapeResults = await _httpClient.GetAsync(
                    agentEndpointUrl
                    + scrapeUrl
                        .Replace("{0}", lastSuccessfulScrapeEpoch.ToString())
                        .Replace("{1}", (lastSuccessfulScrapeEpoch + millisecondsToScrape).ToString()),
                    cancellationToken);

                timer.Stop();
                _logger.LogInformation("scraping took {Seconds} seconds", timer.Elapsed.Seconds);

                if (!scrapeResults.IsSuccessStatusCode)
                {
                    var error = await scrapeResults.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("no metadata found for given date range: {Error}", error);
                    lastSuccessfulScrapeEpoch += millisecondsToScrape;
                    continue;
                }

                var content = await scrapeResults.Content.ReadAsStringAsync(cancellationToken);

                var metaDatasToQuery = JsonSerializer.Deserialize<List<ScrapeMetadata>>(content);

                _logger.LogInformation("Found {Count} entries for: {Date}",
                    metaDatasToQuery.Count,
                    lastSuccessfulScrapeEpoch.ToString());

                for (int i = 0; i < metaDatasToQuery.Count; i++)
                {
                    ScrapeMetadata metadata = metaDatasToQuery[i];
                    _logger.LogDebug("querying key: {Key}", metadata.Key);

                    timer.Reset();
                    timer.Start();

                    var newGroup = await _httpClient.GetAsync(
                        agentEndpointUrl + metadataUrl.Replace("{0}", metadata.Key),
                        cancellationToken);

                    timer.Stop();
                    _logger.LogDebug("Took {Seconds} to query", timer.Elapsed.TotalSeconds);

                    if (!newGroup.IsSuccessStatusCode)
                    {
                        _logger.LogError(
                            "bad response received from Agent: {StatusCode} {ReasonPhrase}",
                            newGroup.StatusCode,
                            newGroup.ReasonPhrase);

                        continue;
                    }

                    OpenAlprWebhook.Group group;

                    try
                    {
                        timer.Reset();
                        timer.Start();
                        _logger.LogDebug("deserializing key: {Key}", metadata.Key);

                        group = await JsonSerializer.DeserializeAsync<OpenAlprWebhook.Group>(
                            await newGroup.Content.ReadAsStreamAsync(cancellationToken),
                            cancellationToken: cancellationToken);

                        timer.Stop();
                        _logger.LogDebug("took {Seconds} to deserialize.", timer.Elapsed.TotalSeconds);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "unable to deserialize response from Agent for meta id: {Metadatakey}", metadata.Key);
                        continue;
                    }

                    try
                    {
                        _logger.LogInformation("date: {Fate} querying: {Key}", DateTimeOffset.FromUnixTimeMilliseconds(group.EpochStart).ToString(), metadata.Key);

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
                        _logger.LogDebug("took {Seconds} to process.", timer.Elapsed.TotalSeconds);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "failed to parse bulk import request.");
                    }

                    timer.Reset();
                    timer.Start();

                    _logger.LogDebug("saving agent status, last scrape {ScrapeEpoch}", group.EpochStart);

                    lastSuccessfulScrapeEpoch = group.EpochStart;

                    timer.Stop();

                    _logger.LogDebug("took {Seconds} to update agent status.", timer.Elapsed.TotalSeconds);
                }

                lastSuccessfulScrapeEpoch += millisecondsToScrape;
            }

            if (lastSuccessfulScrapeEpoch > startDate)
            {
                lastSuccessfulScrapeEpoch = startDate;
            }

            _logger.LogInformation("finished OpenALPR Agent scrape.");

            return lastSuccessfulScrapeEpoch;
        }

        public void ScheduleAgentImageScraping(List<string> plateGroupIds)
        {
            _logger.LogInformation("searching for plates with missings images");

            foreach (var plateGroupId in plateGroupIds)
            {
                _imageRetriever.TryAddJob(plateGroupId);
            }

            _logger.LogInformation("jobs added successfully.");
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
                throw new ArgumentException($"unable to get earliest group epoch: Status code: {result.StatusCode} Reason: {result.ReasonPhrase}");
            }

            var content = await result.Content.ReadAsStringAsync(cancellationToken);

            var firstEpochMs = Regex.Match(content, "earliest date epoch: ([0-9]+)").Groups[1].Value;

            return long.Parse(firstEpochMs);
        }
    }
}
