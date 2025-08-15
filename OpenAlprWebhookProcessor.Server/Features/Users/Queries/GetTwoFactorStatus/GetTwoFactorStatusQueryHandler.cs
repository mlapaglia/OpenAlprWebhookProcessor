using Mediator;
using Microsoft.AspNetCore.Identity;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetTwoFactorStatus
{
    public class GetTwoFactorStatusQueryHandler : IQueryHandler<GetTwoFactorStatusQuery, TwoFactorStatusResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetTwoFactorStatusQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async ValueTask<TwoFactorStatusResponse> Handle(GetTwoFactorStatusQuery request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(request.User);
            if (user == null)
                throw new AppException("User not found");

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            var hasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;

            return new TwoFactorStatusResponse(isTwoFactorEnabled, hasAuthenticator);
        }
    }
}