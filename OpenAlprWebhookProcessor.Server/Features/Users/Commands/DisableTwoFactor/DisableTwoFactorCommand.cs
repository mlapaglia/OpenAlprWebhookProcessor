using Mediator;
using System.Security.Claims;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.DisableTwoFactor
{
    public class DisableTwoFactorCommand : ICommand
    {
        public ClaimsPrincipal User { get; set; } = default!;

        public DisableTwoFactorCommand(ClaimsPrincipal user)
        {
            User = user;
        }
    }
}