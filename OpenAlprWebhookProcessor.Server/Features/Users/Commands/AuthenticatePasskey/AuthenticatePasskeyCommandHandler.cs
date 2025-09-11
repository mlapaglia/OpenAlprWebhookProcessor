using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.AuthenticatePasskey
{
    public class AuthenticatePasskeyCommandHandler : IQueryHandler<AuthenticatePasskeyCommand, AuthenticatePasskeyResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFido2 _fido2;
        private readonly UsersContext _context;
        private readonly IMemoryCache _cache;

        public AuthenticatePasskeyCommandHandler(
            UserManager<ApplicationUser> userManager,
            IFido2 fido2,
            UsersContext context,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _fido2 = fido2;
            _context = context;
            _cache = cache;
        }

        public async ValueTask<AuthenticatePasskeyResponse> Handle(AuthenticatePasskeyCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                throw new AppException("User not found");

            var existingCredentials = await _context.PasskeyCredentials
                .Where(c => c.UserId == user.Id)
                .Select(c => new PublicKeyCredentialDescriptor(Convert.FromBase64String(c.CredentialId)))
                .ToListAsync(cancellationToken);

            if (!existingCredentials.Any())
                throw new AppException("No passkeys registered for this user");

            var options = _fido2.GetAssertionOptions(
                existingCredentials,
                UserVerificationRequirement.Preferred);

            var cacheKey = $"passkey_authentication_{user.Id}";
            _cache.Set(cacheKey, options, TimeSpan.FromMinutes(5));

            return new AuthenticatePasskeyResponse(options);
        }
    }
}
