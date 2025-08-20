using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Ignores.Commands.AddIgnore
{
    public class AddIgnoreCommandHandler : ICommandHandler<AddIgnoreCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddIgnoreCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Unit> Handle(AddIgnoreCommand command, CancellationToken cancellationToken = default)
        {
            var ignore = command.Ignore;

            var existingIgnores = await _unitOfWork.Ignores.FindAsync(
                x => x.PlateNumber == ignore.PlateNumber, 
                cancellationToken);

            if (existingIgnores.Any())
            {
                throw new ArgumentException("ignore already exists");
            }

            var addedIgnore = new Data.Ignore()
            {
                Description = ignore.Description,
                IsStrictMatch = ignore.StrictMatch,
                PlateNumber = ignore.PlateNumber,
            };

            await _unitOfWork.Ignores.AddAsync(addedIgnore, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}