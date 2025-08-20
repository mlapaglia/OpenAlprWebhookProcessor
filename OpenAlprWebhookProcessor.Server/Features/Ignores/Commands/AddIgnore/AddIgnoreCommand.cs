using Mediator;
using OpenAlprWebhookProcessor.Features.Ignores.Queries.GetIgnores;

namespace OpenAlprWebhookProcessor.Features.Ignores.Commands.AddIgnore
{
    public class AddIgnoreCommand : ICommand
    {
        public IgnoreDto Ignore { get; set; }

        public AddIgnoreCommand(IgnoreDto ignore)
        {
            Ignore = ignore;
        }
    }
} 