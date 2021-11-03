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
                lastSuccessfulScrape = await GetEarliestGroupEpochAsync(agent, cancellationToken);
            }
            else
            {
                lastSuccessfulScrape = DateTimeOffset.FromUnixTimeMilliseconds(agent.LastSuccessfulScrapeEpoch);
            }

            var startDate = DateTimeOffset.UtcNow;
            while (startDate > lastSuccessfulScrape)
            {
                _logger.LogInformation("Scraping between "
                   + lastSuccessfulScrape.ToUnixTimeMilliseconds().ToString()
                   + " and "
                   + lastSuccessfulScrape.AddMinutes(minutesToScrape).ToUnixTimeMilliseconds().ToString());

                var scrapeResults = await _httpClient.GetAsync(
                    agent.EndpointUrl
                    + scrapeUrl
                        .Replace("{0}", lastSuccessfulScrape.ToUnixTimeMilliseconds().ToString())
                        .Replace("{1}", lastSuccessfulScrape.AddMinutes(minutesToScrape).ToUnixTimeMilliseconds().ToString()),
                    cancellationToken);

                if (!scrapeResults.IsSuccessStatusCode)
                {
                    var error = await scrapeResults.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("no metadata found for given date range: {error}", error);
                    lastSuccessfulScrape = lastSuccessfulScrape.AddMinutes(minutesToScrape);
                    continue;
                }

                var content = await scrapeResults.Content.ReadAsStringAsync(cancellationToken);

                var metaDatasToQuery = JsonSerializer.Deserialize<List<ScrapeMetadata>>(content);

                _logger.LogInformation("Found " + metaDatasToQuery.Count + " entries for: " + lastSuccessfulScrape.ToString());

                foreach (var metadata in metaDatasToQuery)
                {
                    var timer = new Stopwatch();
                    timer.Start();

                    var newGroup = await _httpClient.GetAsync(
                        agent.EndpointUrl + metadataUrl.Replace("{0}", metadata.Key),
                        cancellationToken);

                    timer.Stop();
                    _logger.LogDebug("Took {seconds} to query", timer.Elapsed.TotalSeconds);

                    if (!newGroup.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Bad response received from Agent: {newGroup.StatusCode} {newGroup.ReasonPhrase}");
                        continue;
                    }

                    Group group;

                    try
                    {
                        timer.Reset();
                        timer.Start();
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

                    _logger.LogInformation("date: " + DateTimeOffset.FromUnixTimeMilliseconds(group.EpochStart).ToString() + " querying: " + metadata.Key);

                    try
                    {
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
                    agent.LastSuccessfulScrapeEpoch = group.EpochStart;
                    await _processorContext.SaveChangesAsync(cancellationToken);
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
            var plateGroupIds = await _processorContext.PlateGroups
                .Where(x => x.VehicleJpeg == null || x.PlateJpeg == null)
                .OrderByDescending(x => x.ReceivedOnEpoch)
                .Select(x => x.OpenAlprUuid)
                .ToListAsync(cancellationToken);

            foreach (var plateGroupId in plateGroupIds)
            {
                _imageRetriever.AddJob(plateGroupId);
            }
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
                throw new ArgumentException("Unable to get earliest group epoch");
            }

            var content = await result.Content.ReadAsStringAsync(cancellationToken);

            var firstEpochMs = Regex.Match(content, "Earliest date epoch: ([0-9]+)").Groups[1].Value;

            return DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(firstEpochMs));
        }
    }
}
