using Mediator;
using Microsoft.AspNetCore.Identity;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, ApplicationUser>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetUserByIdQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async ValueTask<ApplicationUser> Handle(GetUserByIdQuery request, CancellationToken cancellationToken = default)
        {
            return await _userManager.FindByIdAsync(request.Id.ToString());
        }
    }
}