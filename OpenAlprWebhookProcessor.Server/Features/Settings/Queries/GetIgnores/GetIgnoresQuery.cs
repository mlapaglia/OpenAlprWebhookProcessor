using MediatR;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetIgnores
{
    public class GetIgnoresQuery : IRequest<List<IgnoreDto>>
    {
    }
} 