using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.ImageRelay.GetImage
{
    public interface IGetImageHandler
    {
        Task<Stream> GetImageFromLocalAsync(
            string imageId,
            CancellationToken cancellationToken);

        Task<Stream> GetCropImageFromLocalAsync(
            string imageId,
            CancellationToken cancellationToken);

        Task<byte[]> GetImageFromAgentAsync(
            string imageId,
            CancellationToken cancellationToken);

        Task<byte[]> GetCropImageFromAgentAsync(
            string imageId,
            CancellationToken cancellationToken);
    }
}
