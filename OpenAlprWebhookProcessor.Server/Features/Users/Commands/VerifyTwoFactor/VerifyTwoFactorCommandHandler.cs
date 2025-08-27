using Mediator;
using Microsoft.AspNetCore.Identity;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.VerifyTwoFactor
{
    public class VerifyTwoFactorCommandHandler : IQueryHandler<VerifyTwoFactorCommand, AuthenticateResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public VerifyTwoFactorCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async ValueTask<AuthenticateResponse> Handle(VerifyTwoFactorCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserId))
                throw new AppException("Invalid user");
                
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                throw new AppException("Invalid user");

            var isValidToken = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider,
                request.Code);

            if (!isValidToken)
                throw new AppException("Invalid verification code");

            await _signInManager.SignInAsync(user, request.RememberMe);

            return new AuthenticateResponse(user);
        }
    }
}