namespace OpenAlprWebhookProcessor.Features.Users.Commands.CompletePasskeyRegistration
{
    public record CompletePasskeyRegistrationRequest(string AttestationResponse, string? Name = null);
}