using Mediator;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.AuthenticatePasskey
{
    public record AuthenticatePasskeyCommand(string Username) : IQuery<AuthenticatePasskeyResponse>;
}
