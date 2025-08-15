using Mediator;
using System.Security.Claims;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetTwoFactorStatus
{
    public class GetTwoFactorStatusQuery : IQuery<TwoFactorStatusResponse>
    {
        public ClaimsPrincipal User { get; set; } = default!;

        public GetTwoFactorStatusQuery(ClaimsPrincipal user)
        {
            User = user;
        }
    }
}