using MediatR;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Services;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;
        private readonly IPasswordService _passwordService;

        public UpdateUserCommandHandler(
            IUsersUnitOfWork usersUnitOfWork,
            IPasswordService passwordService)
        {
            _usersUnitOfWork = usersUnitOfWork;
            _passwordService = passwordService;
        }

        public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _usersUnitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);

            if (user == null)
            {
                throw new AppException("User not found");
            }

            if (!string.IsNullOrWhiteSpace(request.Username) && user.Username != request.Username)
            {
                var usernameExists = await _usersUnitOfWork.Users.UsernameExistsAsync(request.Username, cancellationToken);
                if (usernameExists)
                {
                    throw new AppException("Username " + request.Username + " is already taken");
                }

                user.Username = request.Username;
            }

            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                user.FirstName = request.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                user.LastName = request.LastName;
            }

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                _passwordService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _usersUnitOfWork.Users.Update(user);
            await _usersUnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
} 