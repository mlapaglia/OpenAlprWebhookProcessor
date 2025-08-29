using Mediator;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.CompletePasskeyAuthentication
{
    public record CompletePasskeyAuthenticationCommand(
        string Username,
        string AssertionResponse,
        bool RememberMe = false) : IQuery<UserDto>;
}
