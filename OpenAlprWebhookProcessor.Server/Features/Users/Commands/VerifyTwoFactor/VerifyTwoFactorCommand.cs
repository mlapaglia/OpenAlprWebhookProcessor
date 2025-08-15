using Mediator;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.VerifyTwoFactor
{
    public class VerifyTwoFactorCommand : IQuery<AuthenticateResponse>
    {
        public string UserId { get; set; } = default!;
        public string Code { get; set; } = default!;
        public bool RememberMe { get; set; }

        public VerifyTwoFactorCommand(string userId, string code, bool rememberMe)
        {
            UserId = userId;
            Code = code;
            RememberMe = rememberMe;
        }
    }
}