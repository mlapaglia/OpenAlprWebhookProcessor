using MediatR;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.CanRegister
{
    public class CanRegisterQueryHandler : IRequestHandler<CanRegisterQuery, bool>
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;

        public CanRegisterQueryHandler(IUsersUnitOfWork usersUnitOfWork)
        {
            _usersUnitOfWork = usersUnitOfWork;
        }

        public async Task<bool> Handle(CanRegisterQuery request, CancellationToken cancellationToken)
        {
            var users = await _usersUnitOfWork.Users.GetAllAsync(cancellationToken);
            return users.Count() == 0;
        }
    }
} 