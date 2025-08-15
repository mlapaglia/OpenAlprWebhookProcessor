using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.DeleteIgnore
{
    public class DeleteIgnoreCommandHandler : ICommandHandler<DeleteIgnoreCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteIgnoreCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Unit> Handle(DeleteIgnoreCommand command, CancellationToken cancellationToken = default)
        {
            var ignore = await _unitOfWork.Ignores.GetByIdAsync(command.Id, cancellationToken);
            
            if (ignore == null)
            {
                throw new InvalidOperationException($"Ignore with ID {command.Id} not found");
            }

            _unitOfWork.Ignores.Delete(ignore);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
