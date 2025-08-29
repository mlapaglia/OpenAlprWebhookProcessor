using Mediator;
using System.Security.Claims;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.RegisterPasskey
{
    public record RegisterPasskeyCommand(ClaimsPrincipal User, string? Name = null) : IQuery<RegisterPasskeyResponse>;
}
