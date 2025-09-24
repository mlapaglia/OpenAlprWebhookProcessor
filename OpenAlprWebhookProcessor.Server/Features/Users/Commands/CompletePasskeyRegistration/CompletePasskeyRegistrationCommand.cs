using Mediator;
using System.Security.Claims;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.CompletePasskeyRegistration
{
    public record CompletePasskeyRegistrationCommand(
        ClaimsPrincipal User,
        string AttestationResponse,
        string? Name = null) : IQuery<CompletePasskeyRegistrationResponse>;
}
