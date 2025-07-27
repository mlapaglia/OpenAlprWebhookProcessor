using OpenAlprWebhookProcessor.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.ImageCompression
{
    public interface IImageCompressionService
    {
        Task<byte[]> GetImageFromAgentAsync(
            Agent agent,
            string imageId,
            CancellationToken cancellationToken = default);

        Task<byte[]> GetCropImageFromAgentAsync(
            Agent agent,
            string imageId,
            CancellationToken cancellationToken = default);


        Task<byte[]> GetCropImageFromAgentAsync(
            Agent agent,
            string imageId,
            string plateCoordinates,
            CancellationToken cancellationToken = default);
    }
}
