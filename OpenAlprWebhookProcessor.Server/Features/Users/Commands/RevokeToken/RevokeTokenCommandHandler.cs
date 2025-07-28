using Mediator;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.RevokeToken
{
    public class RevokeTokenCommandHandler : IQueryHandler<RevokeTokenCommand, bool>
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;

        public RevokeTokenCommandHandler(IUsersUnitOfWork usersUnitOfWork)
        {
            _usersUnitOfWork = usersUnitOfWork;
        }

        public async ValueTask<bool> Handle(RevokeTokenCommand request, CancellationToken cancellationToken = default)
        {
            var user = await _usersUnitOfWork.Users.GetByRefreshTokenAsync(request.Token, cancellationToken);

            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == request.Token);

            if (!refreshToken.IsActive) return false;

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = request.IpAddress;

            _usersUnitOfWork.Users.Update(user);
            await _usersUnitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
} 