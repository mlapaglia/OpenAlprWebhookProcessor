using MediatR;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetDebugPlates
{
    public class GetDebugPlatesQuery : IRequest<string>
    {
        public bool OnlyFailedPlateGroups { get; set; }

        public GetDebugPlatesQuery(bool onlyFailedPlateGroups)
        {
            OnlyFailedPlateGroups = onlyFailedPlateGroups;
        }
    }
} 