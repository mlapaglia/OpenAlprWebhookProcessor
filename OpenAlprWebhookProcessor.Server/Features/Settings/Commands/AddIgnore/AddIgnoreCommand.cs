using MediatR;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetIgnores;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.AddIgnore
{
    public class AddIgnoreCommand : IRequest
    {
        public IgnoreDto Ignore { get; set; }

        public AddIgnoreCommand(IgnoreDto ignore)
        {
            Ignore = ignore;
        }
    }
} 