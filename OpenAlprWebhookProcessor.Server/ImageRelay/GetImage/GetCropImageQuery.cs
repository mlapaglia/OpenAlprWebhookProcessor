using MediatR;
using System.IO;

namespace OpenAlprWebhookProcessor.ImageRelay.GetImage
{
    public class GetCropImageQuery : IRequest<Stream>
    {
        public string ImageId { get; set; }

        public GetCropImageQuery(string imageId)
        {
            ImageId = imageId;
        }
    }
} 