using Mediator;
using System.IO;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.GetImage
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