using MediatR;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlateFilters;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlateFilters
{
    public class GetPlateFiltersQuery : IRequest<GetLicensePlateFiltersResponse>
    {
    }
} 