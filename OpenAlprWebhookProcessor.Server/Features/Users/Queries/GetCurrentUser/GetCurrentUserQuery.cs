using Mediator;
using System.Security.Claims;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetCurrentUser
{
    public class GetCurrentUserQuery : IQuery<UserDto?>
    {
        public ClaimsPrincipal User { get; set; } = default!;

        public GetCurrentUserQuery(ClaimsPrincipal user)
        {
            User = user;
        }
    }
}