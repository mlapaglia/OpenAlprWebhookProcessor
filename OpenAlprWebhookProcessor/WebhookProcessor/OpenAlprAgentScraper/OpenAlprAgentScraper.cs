using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprAgentScraper
{
    public class OpenAlprAgentScraper
    {
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

            var currentRequestEndEpoch = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var scrapeResults = await _httpClient.GetAsync(
                agent.EndpointUrl
                + scrapeUrl
                    .Replace("{0}", agent.LastSuccessfulScrapeEpoch.ToString())
                    .Replace("{1}", currentRequestEndEpoch.ToString()),
                cancellationToken);

            if (!scrapeResults.IsSuccessStatusCode)
            {
                throw new ArgumentException("no metadata found for given date range");
            }

            var content = await scrapeResults.Content.ReadAsStringAsync(cancellationToken);

            var metaDatasToQuery = JsonSerializer.Deserialize<List<ScrapeMetadata>>(content);

            foreach (var metadata in metaDatasToQuery)
            {
                var newGroup = await _httpClient.GetAsync(
                    metadataUrl.Replace("{0}", metadata.Key),
                    cancellationToken);

                if (!newGroup.IsSuccessStatusCode)
                {
                    _logger.LogError("Unable to parse Group with Id: " + metadata.Key);
                    continue;
                }

                var newGroupResult = await newGroup.Content.ReadAsStringAsync(cancellationToken);

                await _groupWebhookHandler.HandleWebhookAsync(
                    new Webhook
                    {
                        Group = JsonSerializer.Deserialize<Group>(newGroupResult)
                    },
                    cancellationToken);
            }

            agent.LastSuccessfulScrapeEpoch = currentRequestEndEpoch;
            await _processorContext.SaveChangesAsync(cancellationToken);
        }
    }
}
