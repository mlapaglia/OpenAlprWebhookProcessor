using Fido2NetLib;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.AuthenticatePasskey
{
    public record AuthenticatePasskeyResponse(AssertionOptions Options);
}
