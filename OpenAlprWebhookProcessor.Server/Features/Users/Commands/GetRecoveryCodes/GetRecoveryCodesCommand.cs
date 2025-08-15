using Mediator;
using System.Security.Claims;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.GetRecoveryCodes
{
    public class GetRecoveryCodesCommand : IQuery<GetRecoveryCodesResponse>
    {
        public ClaimsPrincipal User { get; set; } = default!;

        public GetRecoveryCodesCommand(ClaimsPrincipal user)
        {
            User = user;
        }
    }
}