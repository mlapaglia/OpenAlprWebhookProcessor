using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertIgnores
{
    public class UpsertIgnoresCommandHandler : ICommandHandler<UpsertIgnoresCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpsertIgnoresCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Unit> Handle(UpsertIgnoresCommand command, CancellationToken cancellationToken = default)
        {
            var ignores = command.Ignores.Where(x => !string.IsNullOrWhiteSpace(x.PlateNumber)).ToList();

            var dbIgnores = (await _unitOfWork.Ignores.GetAllAsync(cancellationToken)).ToList();

            var ignoresToRemove = dbIgnores.Where(p => !ignores.Any(p2 => p2.Id == p.Id));

            _unitOfWork.Ignores.DeleteRange(ignoresToRemove);

            var ignoresToUpdate = dbIgnores.Where(x => ignores.Any(p2 => p2.Id == x.Id));

            foreach (var ignoreToUpdate in ignoresToUpdate)
            {
                var updatedIgnore = ignores.First(x => x.Id == ignoreToUpdate.Id);

                ignoreToUpdate.Description = updatedIgnore.Description;
                ignoreToUpdate.IsStrictMatch = updatedIgnore.StrictMatch;
                ignoreToUpdate.PlateNumber = updatedIgnore.PlateNumber;

                _unitOfWork.Ignores.Update(ignoreToUpdate);
            }

            var ignoresToAdd = ignores.Where(x => !dbIgnores.Any(p2 => p2.Id == x.Id));

            foreach (var ignoreToAdd in ignoresToAdd)
            {
                var addedIgnore = new Data.Ignore()
                {
                    Description = ignoreToAdd.Description,
                    IsStrictMatch = ignoreToAdd.StrictMatch,
                    PlateNumber = ignoreToAdd.PlateNumber,
                };

                await _unitOfWork.Ignores.AddAsync(addedIgnore, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}