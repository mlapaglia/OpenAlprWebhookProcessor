using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using System.Text.Json;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.CompletePasskeyAuthentication
{
    public class CompletePasskeyAuthenticationCommandHandler : IQueryHandler<CompletePasskeyAuthenticationCommand, UserDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IFido2 _fido2;
        private readonly UsersContext _context;
        private readonly IMemoryCache _cache;

        public CompletePasskeyAuthenticationCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IFido2 fido2,
            UsersContext context,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fido2 = fido2;
            _context = context;
            _cache = cache;
        }

        public async ValueTask<UserDto> Handle(CompletePasskeyAuthenticationCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                throw new AppException("User not found");
            }

            try
            {
                var assertionResponse = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(request.AssertionResponse);
                if (assertionResponse == null)
                    throw new AppException("Invalid assertion response");

                var credentialId = Convert.ToBase64String(assertionResponse.Id);
                var credential = await _context.PasskeyCredentials
                    .FirstOrDefaultAsync(c => c.CredentialId == credentialId && c.UserId == user.Id, cancellationToken);

                if (credential == null)
                    throw new AppException("Credential not found");

                var optionsCacheKey = $"passkey_authentication_{user.Id}";
                var originalOptions = _cache.Get<AssertionOptions>(optionsCacheKey);

                if (originalOptions == null)
                    throw new AppException("Authentication session expired or invalid. Please try again.");

                _cache.Remove(optionsCacheKey);

                var result = await _fido2.MakeAssertionAsync(
                    assertionResponse,
                    originalOptions,
                    credential.PublicKey,
                    credential.SignatureCounter,
                    async (args, cancellationToken) =>
                    {
                        return credential.UserHandle.SequenceEqual(args.UserHandle);
                    },
                    cancellationToken: cancellationToken);

                if (result.Status != "ok")
                    throw new AppException($"Failed to authenticate with passkey: {result.ErrorMessage}");

                credential.SignatureCounter = result.Counter;
                await _context.SaveChangesAsync(cancellationToken);

                await _signInManager.SignInAsync(user, request.RememberMe);

                return new UserDto
                {
                    FirstName = user.FirstName,
                    Id = user.Id,
                    TwoFactorEnabled = false,
                    HasPasskeys = true,
                    LastName = user.LastName,
                    Username = user.UserName,
                };
            }
            catch (Exception ex)
            {
                throw new AppException($"Passkey authentication failed: {ex.Message}");
            }
        }
    }
}
