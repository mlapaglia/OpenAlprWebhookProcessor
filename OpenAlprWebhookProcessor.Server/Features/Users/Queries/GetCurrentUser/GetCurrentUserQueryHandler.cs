using Mediator;
using Microsoft.AspNetCore.Identity;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetCurrentUser
{
    public class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, UserDto?>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetCurrentUserQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async ValueTask<UserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken = default)
        {
            if (request.User == null || !request.User.Identity.IsAuthenticated)
            {
                return null;
            }

            var user = await _userManager.GetUserAsync(request.User);
            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.UserName
            };
        }
    }
}