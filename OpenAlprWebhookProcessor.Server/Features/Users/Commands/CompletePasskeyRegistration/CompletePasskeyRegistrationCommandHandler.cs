using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using System.Text.Json;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.CompletePasskeyRegistration
{
    public class CompletePasskeyRegistrationCommandHandler : IQueryHandler<CompletePasskeyRegistrationCommand, CompletePasskeyRegistrationResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFido2 _fido2;
        private readonly UsersContext _context;
        private readonly IMemoryCache _cache;

        public CompletePasskeyRegistrationCommandHandler(
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

        public async ValueTask<CompletePasskeyRegistrationResponse> Handle(CompletePasskeyRegistrationCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(request.User);
            if (user == null)
                throw new AppException("User not found");

            try
            {
                var attestationResponse = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(request.AttestationResponse);
                if (attestationResponse == null)
                    throw new AppException("Invalid attestation response");

                var optionsCacheKey = $"passkey_registration_{user.Id}";
                var originalOptions = _cache.Get<CredentialCreateOptions>(optionsCacheKey);

                if (originalOptions == null)
                    throw new AppException("Registration session expired or invalid. Please try again.");

                _cache.Remove(optionsCacheKey);

                var result = await _fido2.MakeNewCredentialAsync(
                    new MakeNewCredentialParams
                    {
                        AttestationResponse = attestationResponse,
                        OriginalOptions = originalOptions,
                        IsCredentialIdUniqueToUserCallback = async (args, cancellationToken) =>
                        {
                            var existingCredential = await _context.PasskeyCredentials
                                .FirstOrDefaultAsync(c => c.CredentialId == Convert.ToBase64String(args.CredentialId), cancellationToken);
                            return existingCredential == null;
                        }
                    });

                var credential = new PasskeyCredential
                {
                    UserId = user.Id,
                    CredentialId = Convert.ToBase64String(result.Id),
                    PublicKey = result.PublicKey,
                    UserHandle = result.User.Id,
                    SignatureCounter = result.SignCount,
                    CredType = result.Type.ToString(),
                    RegDate = DateTime.UtcNow,
                    AaGuid = result.AaGuid.ToString(),
                    Name = request.Name ?? $"Passkey {DateTime.UtcNow:yyyy-MM-dd HH:mm}"
                };

                _context.PasskeyCredentials.Add(credential);
                await _context.SaveChangesAsync(cancellationToken);

                return new CompletePasskeyRegistrationResponse("Passkey registered successfully", true);
            }
            catch (Exception ex)
            {
                return new CompletePasskeyRegistrationResponse($"Failed to register passkey: {ex.Message}", false);
            }
        }
    }
}
