using Mediator;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.CanRegister
{
    public class CanRegisterQueryHandler : IQueryHandler<CanRegisterQuery, bool>
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;

        public CanRegisterQueryHandler(IUsersUnitOfWork usersUnitOfWork)
        {
            _usersUnitOfWork = usersUnitOfWork;
        }

        public async ValueTask<bool> Handle(CanRegisterQuery request, CancellationToken cancellationToken = default)
        {
            return !await _usersUnitOfWork.Users.AnyAsync(x => true, cancellationToken);
        }
    }
} 