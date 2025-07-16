using MediatR;
using System.IO;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.GetImage
{
    public class GetImageQuery : IRequest<Stream>
    {
        public string ImageId { get; set; }

        public GetImageQuery(string imageId)
        {
            ImageId = imageId;
        }
    }
} 