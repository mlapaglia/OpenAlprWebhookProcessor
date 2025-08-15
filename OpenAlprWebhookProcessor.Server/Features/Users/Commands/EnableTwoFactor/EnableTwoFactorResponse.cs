using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.EnableTwoFactor
{
    public class EnableTwoFactorResponse
    {
        public string Message { get; set; } = default!;
        public IEnumerable<string> RecoveryCodes { get; set; } = default!;

        public EnableTwoFactorResponse(string message, IEnumerable<string> recoveryCodes)
        {
            Message = message;
            RecoveryCodes = recoveryCodes;
        }
    }
}