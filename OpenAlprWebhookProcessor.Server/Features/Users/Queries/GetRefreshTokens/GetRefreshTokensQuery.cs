using Mediator;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetRefreshTokens
{
    public class GetRefreshTokensQuery : IQuery<List<RefreshToken>>
    {
        public int UserId { get; set; }

        public GetRefreshTokensQuery(int userId)
        {
            UserId = userId;
        }
    }
} 