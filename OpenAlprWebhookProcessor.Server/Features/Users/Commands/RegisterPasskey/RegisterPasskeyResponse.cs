using Fido2NetLib;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.RegisterPasskey
{
    public record RegisterPasskeyResponse(CredentialCreateOptions Options);
}
