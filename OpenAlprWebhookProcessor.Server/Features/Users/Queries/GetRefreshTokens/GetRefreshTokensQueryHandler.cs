using MediatR;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetRefreshTokens
{
    public class GetRefreshTokensQueryHandler : IRequestHandler<GetRefreshTokensQuery, List<RefreshToken>>
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;

        public GetRefreshTokensQueryHandler(IUsersUnitOfWork usersUnitOfWork)
        {
            _usersUnitOfWork = usersUnitOfWork;
        }

        public async Task<List<RefreshToken>> Handle(GetRefreshTokensQuery request, CancellationToken cancellationToken)
        {
            var user = await _usersUnitOfWork.Users.GetByIdWithRefreshTokensAsync(request.UserId, cancellationToken);
            return user?.RefreshTokens;
        }
    }
} 