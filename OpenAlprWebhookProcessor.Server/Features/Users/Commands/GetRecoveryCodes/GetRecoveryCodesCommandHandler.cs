using Mediator;
using Microsoft.AspNetCore.Identity;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.GetRecoveryCodes
{
    public class GetRecoveryCodesCommandHandler : IQueryHandler<GetRecoveryCodesCommand, GetRecoveryCodesResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetRecoveryCodesCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async ValueTask<GetRecoveryCodesResponse> Handle(GetRecoveryCodesCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(request.User);
            if (user == null)
                throw new AppException("User not found");

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            return new GetRecoveryCodesResponse(recoveryCodes);
        }
    }
}