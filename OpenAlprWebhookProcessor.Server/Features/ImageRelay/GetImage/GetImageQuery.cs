using Mediator;
using System.IO;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.GetImage
{
    public class GetImageQuery : IQuery<Stream>
    {
        public string ImageId { get; set; }

        public GetImageQuery(string imageId)
        {
            ImageId = imageId;
        }
    }
} 