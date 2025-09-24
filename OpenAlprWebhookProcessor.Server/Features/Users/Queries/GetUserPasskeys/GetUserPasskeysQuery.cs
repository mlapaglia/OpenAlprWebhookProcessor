using Mediator;
using System.Security.Claims;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetUserPasskeys
{
    public record GetUserPasskeysQuery(ClaimsPrincipal User) : IQuery<GetUserPasskeysResponse>;
}
