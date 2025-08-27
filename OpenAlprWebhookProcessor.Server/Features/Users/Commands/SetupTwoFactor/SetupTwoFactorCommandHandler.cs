using Mediator;
using Microsoft.AspNetCore.Identity;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.SetupTwoFactor
{
    public class SetupTwoFactorCommandHandler : IQueryHandler<SetupTwoFactorCommand, SetupTwoFactorResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UrlEncoder _urlEncoder;

        public SetupTwoFactorCommandHandler(UserManager<ApplicationUser> userManager, UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _urlEncoder = urlEncoder;
        }

        public async ValueTask<SetupTwoFactorResponse> Handle(SetupTwoFactorCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(request.User);
            if (user == null)
                throw new AppException("User not found");

            await _userManager.ResetAuthenticatorKeyAsync(user);
            var key = await _userManager.GetAuthenticatorKeyAsync(user);

            var email = await _userManager.GetEmailAsync(user) ?? user.UserName;
            var qrCodeUri = GenerateQrCodeUri(email, key);

            return new SetupTwoFactorResponse(FormatKey(key), qrCodeUri);
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            const string authenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
            return string.Format(
                authenticatorUriFormat,
                _urlEncoder.Encode("OpenALPR Webhook Processor"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        private static string FormatKey(string unformattedKey)
        {
            var result = new System.Text.StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }
    }
}