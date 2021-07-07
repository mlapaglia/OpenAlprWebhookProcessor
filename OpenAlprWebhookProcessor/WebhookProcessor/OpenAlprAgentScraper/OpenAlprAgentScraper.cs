using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;
using System;
using System.Collections.Generic;
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

        public OpenAlprAgentScraper(
            GroupWebhookHandler groupWebhookHandler,
            ProcessorContext processorContext,
            ILogger<OpenAlprAgentScraper> logger)
        {
            _groupWebhookHandler = groupWebhookHandler;
            _processorContext = processorContext;
            _logger = logger;
            _httpClient = new HttpClient();
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
                var scrapeResults = await _httpClient.GetAsync(
                    agent.EndpointUrl
                    + scrapeUrl
                        .Replace("{0}", lastSuccessfulScrape.ToUnixTimeMilliseconds().ToString())
                        .Replace("{1}", lastSuccessfulScrape.AddMinutes(minutesToScrape).ToUnixTimeMilliseconds().ToString()),
                    cancellationToken);

                if (!scrapeResults.IsSuccessStatusCode)
                {
                    var error = await scrapeResults.Content.ReadAsStringAsync(cancellationToken);
                    throw new ArgumentException("no metadata found for given date range: " + error);
                }

                var content = await scrapeResults.Content.ReadAsStringAsync(cancellationToken);

                var metaDatasToQuery = JsonSerializer.Deserialize<List<ScrapeMetadata>>(content);

                _logger.LogInformation("Found " + metaDatasToQuery.Count + " entries for: " + lastSuccessfulScrape.ToString());

                foreach (var metadata in metaDatasToQuery)
                {
                    var newGroup = await _httpClient.GetAsync(
                        agent.EndpointUrl + metadataUrl.Replace("{0}", metadata.Key),
                        cancellationToken);

                    var group = await JsonSerializer.DeserializeAsync<Group>(
                        await newGroup.Content.ReadAsStreamAsync(cancellationToken),
                        cancellationToken: cancellationToken);

                    _logger.LogInformation("date: " + DateTimeOffset.FromUnixTimeMilliseconds(group.EpochStart).ToString() + " querying: " + metadata.Key);

                    if (!newGroup.IsSuccessStatusCode)
                    {
                        _logger.LogError("Unable to parse Group with Id: " + metadata.Key);
                        continue;
                    }

                    try
                    {
                        await _groupWebhookHandler.HandleWebhookAsync(
                            new Webhook
                            {
                                Group = group,
                            },
                            true,
                            cancellationToken);
                    }
                    catch
                    {
                        _logger.LogError("Failed to parse bulk import request.");
                    }

                    agent.LastSuccessfulScrapeEpoch = group.EpochStart;
                    await _processorContext.SaveChangesAsync(cancellationToken);
                }

                lastSuccessfulScrape = lastSuccessfulScrape.AddMinutes(minutesToScrape);

                if (lastSuccessfulScrape > startDate)
                {
                    lastSuccessfulScrape = startDate;
                }
            }

            _logger.LogInformation("Finished OpenALPR Agent scrape.");
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
