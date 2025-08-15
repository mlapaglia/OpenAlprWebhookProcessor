using Mediator;
using System.Security.Claims;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.EnableTwoFactor
{
    public class EnableTwoFactorCommand : IQuery<EnableTwoFactorResponse>
    {
        public ClaimsPrincipal User { get; set; } = default!;
        public string Code { get; set; } = default!;

        public EnableTwoFactorCommand(ClaimsPrincipal user, string code)
        {
            User = user;
            Code = code;
        }
    }
}