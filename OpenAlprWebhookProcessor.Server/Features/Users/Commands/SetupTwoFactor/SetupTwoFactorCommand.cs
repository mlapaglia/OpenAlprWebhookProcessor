using Mediator;
using System.Security.Claims;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.SetupTwoFactor
{
    public class SetupTwoFactorCommand : IQuery<SetupTwoFactorResponse>
    {
        public ClaimsPrincipal User { get; set; } = default!;

        public SetupTwoFactorCommand(ClaimsPrincipal user)
        {
            User = user;
        }
    }
}