using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetUserPasskeys
{
    public class GetUserPasskeysQueryHandler : IQueryHandler<GetUserPasskeysQuery, GetUserPasskeysResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UsersContext _context;

        public GetUserPasskeysQueryHandler(
            UserManager<ApplicationUser> userManager,
            UsersContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async ValueTask<GetUserPasskeysResponse> Handle(GetUserPasskeysQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(request.User);
            if (user == null)
                throw new AppException("User not found");

            var passkeys = await _context.PasskeyCredentials
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.RegDate)
                .Select(p => new PasskeyDto(
                    p.Id,
                    p.Name ?? "Unnamed Passkey",
                    p.RegDate,
                    p.AaGuid))
                .ToListAsync(cancellationToken);

            return new GetUserPasskeysResponse(passkeys);
        }
    }
}
