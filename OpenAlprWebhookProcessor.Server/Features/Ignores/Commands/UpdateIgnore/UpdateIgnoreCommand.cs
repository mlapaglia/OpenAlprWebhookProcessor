using Mediator;
using OpenAlprWebhookProcessor.Features.Ignores.Queries.GetIgnores;

namespace OpenAlprWebhookProcessor.Features.Ignores.Commands.UpdateIgnore
{
    public class UpdateIgnoreCommand : ICommand
    {
        public IgnoreDto Ignore { get; set; }

        public UpdateIgnoreCommand(IgnoreDto ignore)
        {
            Ignore = ignore;
        }
    }
}
