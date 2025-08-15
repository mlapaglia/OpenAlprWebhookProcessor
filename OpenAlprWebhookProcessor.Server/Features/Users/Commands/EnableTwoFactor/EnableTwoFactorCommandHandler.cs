using Mediator;
using Microsoft.AspNetCore.Identity;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.EnableTwoFactor
{
    public class EnableTwoFactorCommandHandler : IQueryHandler<EnableTwoFactorCommand, EnableTwoFactorResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public EnableTwoFactorCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async ValueTask<EnableTwoFactorResponse> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(request.User);
            if (user == null)
                throw new AppException("User not found");

            var isValidToken = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider,
                request.Code);

            if (!isValidToken)
                throw new AppException("Verification code is invalid");

            await _userManager.SetTwoFactorEnabledAsync(user, true);

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            return new EnableTwoFactorResponse("Two-factor authentication has been enabled", recoveryCodes);
        }
    }
}