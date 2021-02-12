using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.ImageRelay
{
    public class GetImageHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetImageHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<Stream> GetImageFromAgentAsync(
            string imageId,
            CancellationToken cancellationToken)
        {
            var agent = await _processorContext.Agents.FirstOrDefaultAsync(cancellationToken);

            if (agent == null || string.IsNullOrWhiteSpace(agent.EndpointUrl))
            {
                throw new ArgumentException("agent not configured");
            }

            var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(
                Flurl.Url.Combine(
                    agent.EndpointUrl,
                    "/img/",
                    imageId),
                cancellationToken);

            return await result.Content.ReadAsStreamAsync(cancellationToken);
        }

        public async Task<Stream> GetCropImageFromAgentAsync(
            string imageId,
            CancellationToken cancellationToken)
        {
            var agent = await _processorContext.Agents.FirstOrDefaultAsync(cancellationToken);

            if (agent == null || string.IsNullOrWhiteSpace(agent.EndpointUrl))
            {
                throw new ArgumentException("agent not configured");
            }

            var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(
                Flurl.Url.Combine(
                    agent.EndpointUrl,
                    "/crop/",
                    imageId),
                cancellationToken);

            return await result.Content.ReadAsStreamAsync(cancellationToken);
        }
    }
}
