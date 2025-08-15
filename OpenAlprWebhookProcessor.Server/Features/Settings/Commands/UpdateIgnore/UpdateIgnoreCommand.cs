using Mediator;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetIgnores;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.UpdateIgnore
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
