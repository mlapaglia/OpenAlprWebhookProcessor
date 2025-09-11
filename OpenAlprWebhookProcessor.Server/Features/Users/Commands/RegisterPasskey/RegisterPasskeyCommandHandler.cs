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
using System.Text;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.RegisterPasskey
{
    public class RegisterPasskeyCommandHandler : IQueryHandler<RegisterPasskeyCommand, RegisterPasskeyResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFido2 _fido2;
        private readonly UsersContext _context;
        private readonly IMemoryCache _cache;

        public RegisterPasskeyCommandHandler(
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

        public async ValueTask<RegisterPasskeyResponse> Handle(RegisterPasskeyCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(request.User);
            if (user == null)
                throw new AppException("User not found");

            var existingCredentials = await _context.PasskeyCredentials
                .Where(c => c.UserId == user.Id)
                .Select(c => new PublicKeyCredentialDescriptor(Convert.FromBase64String(c.CredentialId)))
                .ToListAsync(cancellationToken);

            // Create user entity for FIDO2
            var fido2User = new Fido2User
            {
                DisplayName = $"{user.FirstName} {user.LastName}".Trim(),
                Name = user.UserName ?? user.Email ?? user.Id.ToString(),
                Id = Encoding.UTF8.GetBytes(user.Id.ToString())
            };

            var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
            {
                User = fido2User,
                ExcludeCredentials = existingCredentials,
                AuthenticatorSelection = AuthenticatorSelection.Default,
                AttestationPreference = AttestationConveyancePreference.None
            });

            var cacheKey = $"passkey_registration_{user.Id}";
            _cache.Set(cacheKey, options, TimeSpan.FromMinutes(5));

            return new RegisterPasskeyResponse(options);
        }
    }
}
