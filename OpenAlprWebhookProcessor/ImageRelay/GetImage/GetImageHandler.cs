using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.ImageRelay
{
    public static class GetImageHandler
    {
        public async static Task<Stream> GetImageFromLocalAsync(
            ProcessorContext processorContext,
            string imageId,
            CancellationToken cancellationToken)
        {
            var plateGroup = await processorContext.PlateGroups.FirstOrDefaultAsync(
                x => x.OpenAlprUuid == imageId,
                cancellationToken);

            if (plateGroup == null)
            {
                throw new ArgumentException("No image found with that id.");
            }

            return new MemoryStream(plateGroup.VehicleJpeg);
        }

        public async static Task<Stream> GetCropImageFromLocalAsync(
            ProcessorContext processorContext,
            string imageId,
            CancellationToken cancellationToken)
        {
            var plateGroup = await processorContext.PlateGroups.FirstOrDefaultAsync(
                x => x.OpenAlprUuid == imageId,
                cancellationToken);

            if (plateGroup == null)
            {
                throw new ArgumentException("No image found with that id.");
            }

            return new MemoryStream(plateGroup.PlateJpeg);
        }

        public async static Task<byte[]> GetImageFromAgentAsync(
            ProcessorContext processorContext,
            string imageId,
            CancellationToken cancellationToken)
        {
            var agent = await processorContext.Agents.FirstOrDefaultAsync(cancellationToken);

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

            return await result.Content.ReadAsByteArrayAsync(cancellationToken);
        }

        public async static Task<byte[]> GetCropImageFromAgentAsync(
            ProcessorContext processorContext,
            string imageId,
            CancellationToken cancellationToken)
        {
            var agent = await processorContext.Agents.FirstOrDefaultAsync(cancellationToken);

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

            return await result.Content.ReadAsByteArrayAsync(cancellationToken);
        }
    }
}
