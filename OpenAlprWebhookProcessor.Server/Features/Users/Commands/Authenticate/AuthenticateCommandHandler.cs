using Mediator;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Services;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.Authenticate
{
    public class AuthenticateCommandHandler : IQueryHandler<AuthenticateCommand, AuthenticateResponse>
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IPasswordService _passwordService;

        public AuthenticateCommandHandler(
            IUsersUnitOfWork usersUnitOfWork,
            IJwtService jwtService,
            IPasswordService passwordService)
        {
            _usersUnitOfWork = usersUnitOfWork;
            _jwtService = jwtService;
            _passwordService = passwordService;
        }

        public async ValueTask<AuthenticateResponse> Handle(AuthenticateCommand request, CancellationToken cancellationToken = default)
        {
            var user = await _usersUnitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken);

            if (user == null)
            {
                return null;
            }

            if (!_passwordService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            var jwtToken = await _jwtService.GenerateJwtTokenAsync(user, cancellationToken);
            var refreshToken = _jwtService.GenerateRefreshToken(request.IpAddress);

            user.RefreshTokens ??= new System.Collections.Generic.List<Users.RefreshToken>();
            user.RefreshTokens.Add(refreshToken);

            _usersUnitOfWork.Users.Update(user);
            await _usersUnitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
        }
    }
} 