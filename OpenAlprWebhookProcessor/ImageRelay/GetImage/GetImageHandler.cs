using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.IO;
using System.Linq;
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
            var vehicleJpeg = await processorContext.PlateGroups
                .Where(x => x.OpenAlprUuid == imageId)
                .Select(x => x.VehicleJpeg)
                .FirstOrDefaultAsync(cancellationToken);

            if (vehicleJpeg == null)
            {
                throw new ArgumentException("No image found with that id.");
            }

            return new MemoryStream(vehicleJpeg);
        }

        public async static Task<Stream> GetCropImageFromLocalAsync(
            ProcessorContext processorContext,
            string imageId,
            CancellationToken cancellationToken)
        {
            var plateJpeg = await processorContext.PlateGroups
                .Where(x => x.OpenAlprUuid == imageId)
                .Select(x => x.PlateJpeg)
                .FirstOrDefaultAsync(cancellationToken);

            if (plateJpeg == null)
            {
                throw new ArgumentException("No image found with that id.");
            }

            return new MemoryStream(plateJpeg);
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

            if (!result.IsSuccessStatusCode)
            {
                throw new ArgumentException("Image not found for that id.");
            }

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

            if (!result.IsSuccessStatusCode)
            {
                throw new ArgumentException("Image not found for that id.");
            }

            return await result.Content.ReadAsByteArrayAsync(cancellationToken);
        }
    }
}
