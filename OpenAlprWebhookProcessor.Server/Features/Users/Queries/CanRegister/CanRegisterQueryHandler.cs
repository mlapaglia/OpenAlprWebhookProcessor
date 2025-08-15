using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.CanRegister
{
    public class CanRegisterQueryHandler : IQueryHandler<CanRegisterQuery, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CanRegisterQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async ValueTask<bool> Handle(CanRegisterQuery request, CancellationToken cancellationToken = default)
        {
            return !await _userManager.Users.AnyAsync(cancellationToken);
        }
    }
} 