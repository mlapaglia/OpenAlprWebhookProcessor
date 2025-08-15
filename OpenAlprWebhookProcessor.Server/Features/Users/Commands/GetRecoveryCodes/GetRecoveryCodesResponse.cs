using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.GetRecoveryCodes
{
    public class GetRecoveryCodesResponse
    {
        public IEnumerable<string> RecoveryCodes { get; set; } = default!;

        public GetRecoveryCodesResponse(IEnumerable<string> recoveryCodes)
        {
            RecoveryCodes = recoveryCodes;
        }
    }
}