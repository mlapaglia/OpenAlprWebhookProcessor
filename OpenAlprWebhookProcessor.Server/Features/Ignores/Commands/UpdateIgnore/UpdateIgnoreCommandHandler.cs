using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Ignores.Commands.UpdateIgnore
{
    public class UpdateIgnoreCommandHandler : ICommandHandler<UpdateIgnoreCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateIgnoreCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Unit> Handle(UpdateIgnoreCommand command, CancellationToken cancellationToken)
        {
            var ignore = command.Ignore;
            var dbIgnore = await _unitOfWork.Ignores.GetByIdAsync(ignore.Id, cancellationToken);
            
            if (dbIgnore == null)
            {
                throw new InvalidOperationException($"Ignore with ID {ignore.Id} not found");
            }

            dbIgnore.PlateNumber = ignore.PlateNumber.ToUpper();
            dbIgnore.Description = ignore.Description;
            dbIgnore.IsStrictMatch = ignore.StrictMatch;

            _unitOfWork.Ignores.Update(dbIgnore);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
