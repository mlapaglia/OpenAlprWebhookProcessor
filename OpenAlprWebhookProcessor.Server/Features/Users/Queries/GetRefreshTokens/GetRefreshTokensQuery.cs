using MediatR;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetRefreshTokens
{
    public class GetRefreshTokensQuery : IRequest<List<RefreshToken>>
    {
        public int UserId { get; set; }

        public GetRefreshTokensQuery(int userId)
        {
            UserId = userId;
        }
    }
} 