using MediatR;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;

        public DeleteUserCommandHandler(IUsersUnitOfWork usersUnitOfWork)
        {
            _usersUnitOfWork = usersUnitOfWork;
        }

        public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken = default)
        {
            var user = await _usersUnitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);

            if (user != null)
            {
                _usersUnitOfWork.Users.Delete(user);
                await _usersUnitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
} 