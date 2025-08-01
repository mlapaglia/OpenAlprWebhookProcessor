using Mediator;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IQueryHandler<RefreshTokenCommand, AuthenticateResponse>
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;
        private readonly IJwtService _jwtService;

        public RefreshTokenCommandHandler(
            IUsersUnitOfWork usersUnitOfWork,
            IJwtService jwtService)
        {
            _usersUnitOfWork = usersUnitOfWork;
            _jwtService = jwtService;
        }

        public async ValueTask<AuthenticateResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken = default)
        {
            var user = await _usersUnitOfWork.Users.GetByRefreshTokenAsync(request.Token, cancellationToken);

            if (user == null)
            {
                return null;
            }

            var refreshToken = user.RefreshTokens.Single(x => x.Token == request.Token);

            if (!refreshToken.IsActive)
            {
                return null;
            }

            var newRefreshToken = _jwtService.GenerateRefreshToken(request.IpAddress);

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = request.IpAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;

            user.RefreshTokens.Add(newRefreshToken);
            _usersUnitOfWork.Users.Update(user);

            await _usersUnitOfWork.SaveChangesAsync(cancellationToken);

            var jwtToken = await _jwtService.GenerateJwtTokenAsync(user, false, cancellationToken);

            return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
        }
    }
} 