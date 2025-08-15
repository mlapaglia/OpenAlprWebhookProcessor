using Mediator;
using Microsoft.AspNetCore.Identity;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.DisableTwoFactor
{
    public class DisableTwoFactorCommandHandler : ICommandHandler<DisableTwoFactorCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DisableTwoFactorCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async ValueTask<Unit> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(request.User);
            if (user == null)
                throw new AppException("User not found");

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);

            return Unit.Value;
        }
    }
}