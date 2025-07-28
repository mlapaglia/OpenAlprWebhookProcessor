using Mediator;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetRefreshTokens
{
    public class GetRefreshTokensQueryHandler : IQueryHandler<GetRefreshTokensQuery, List<RefreshToken>>
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;

        public GetRefreshTokensQueryHandler(IUsersUnitOfWork usersUnitOfWork)
        {
            _usersUnitOfWork = usersUnitOfWork;
        }

        public async ValueTask<List<RefreshToken>> Handle(GetRefreshTokensQuery query, CancellationToken cancellationToken = default)
        {
            var user = await _usersUnitOfWork.Users.GetByIdWithRefreshTokensAsync(query.UserId, cancellationToken);
            return user?.RefreshTokens;
        }
    }
} 