using Mediator;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Services;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IQueryHandler<CreateUserCommand, User>
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;
        private readonly IPasswordService _passwordService;

        public CreateUserCommandHandler(
            IUsersUnitOfWork usersUnitOfWork,
            IPasswordService passwordService)
        {
            _usersUnitOfWork = usersUnitOfWork;
            _passwordService = passwordService;
        }

        public async ValueTask<User> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
        {
            var usernameExists = await _usersUnitOfWork.Users.UsernameExistsAsync(request.Username, cancellationToken);
            if (usernameExists)
            {
                throw new AppException("Username \"" + request.Username + "\" is already taken");
            }

            _passwordService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                RefreshTokens = new System.Collections.Generic.List<Users.RefreshToken>()
            };

            await _usersUnitOfWork.Users.AddAsync(user, cancellationToken);
            await _usersUnitOfWork.SaveChangesAsync(cancellationToken);

            return user;
        }
    }
} 