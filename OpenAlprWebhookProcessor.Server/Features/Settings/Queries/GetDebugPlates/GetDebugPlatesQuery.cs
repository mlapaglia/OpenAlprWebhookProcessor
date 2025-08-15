using Mediator;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetDebugPlates
{
    public class GetDebugPlatesQuery : IQuery<string>
    {
        public bool OnlyFailedPlateGroups { get; set; }

        public GetDebugPlatesQuery(bool onlyFailedPlateGroups)
        {
            OnlyFailedPlateGroups = onlyFailedPlateGroups;
        }
    }
} 