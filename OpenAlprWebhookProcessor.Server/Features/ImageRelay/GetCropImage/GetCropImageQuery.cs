using Mediator;
using System.IO;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.GetCropImage
{
    public class GetCropImageQuery : IQuery<Stream>
    {
        public string ImageId { get; set; }

        public GetCropImageQuery(string imageId)
        {
            ImageId = imageId;
        }
    }
} 