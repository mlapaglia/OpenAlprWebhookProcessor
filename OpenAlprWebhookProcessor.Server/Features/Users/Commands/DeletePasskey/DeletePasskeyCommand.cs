using Mediator;
using System.Security.Claims;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.DeletePasskey
{
    public record DeletePasskeyCommand(ClaimsPrincipal User, int PasskeyId) : IQuery<DeletePasskeyResponse>;
}
