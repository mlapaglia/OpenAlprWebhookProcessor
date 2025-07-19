using MediatR;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
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
            return !await _usersUnitOfWork.Users.AnyAsync(x => true, cancellationToken);
        }
    }
} 