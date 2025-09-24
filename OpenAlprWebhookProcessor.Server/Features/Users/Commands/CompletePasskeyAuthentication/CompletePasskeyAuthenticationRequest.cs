namespace OpenAlprWebhookProcessor.Features.Users.Commands.CompletePasskeyAuthentication
{
    public record CompletePasskeyAuthenticationRequest(string Username, string AssertionResponse, bool RememberMe = false);
}