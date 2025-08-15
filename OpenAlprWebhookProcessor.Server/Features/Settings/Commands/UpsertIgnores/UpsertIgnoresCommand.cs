using Mediator;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetIgnores;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertIgnores
{
    public class UpsertIgnoresCommand : ICommand
    {
        public List<IgnoreDto> Ignores { get; set; }

        public UpsertIgnoresCommand(List<IgnoreDto> ignores)
        {
            Ignores = ignores;
        }
    }
} 