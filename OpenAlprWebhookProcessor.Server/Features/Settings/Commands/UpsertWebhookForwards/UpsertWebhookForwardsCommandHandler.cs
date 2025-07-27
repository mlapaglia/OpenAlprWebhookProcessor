using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertWebhookForwards
{
    public class UpsertWebhookForwardsCommandHandler : IRequestHandler<UpsertWebhookForwardsCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpsertWebhookForwardsCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpsertWebhookForwardsCommand request, CancellationToken cancellationToken = default)
        {
            var webhookForwards = request.WebhookForwards.Where(x => x.Destination != null).ToList();

            var dbForwards = (await _unitOfWork.WebhookForwards.GetAllAsync(cancellationToken)).ToList();

            var forwardsToRemove = dbForwards.Where(p => !webhookForwards.Any(p2 => p2.Id == p.Id));

            _unitOfWork.WebhookForwards.DeleteRange(forwardsToRemove);

            var forwardsToUpdate = dbForwards.Where(x => webhookForwards.Any(p2 => p2.Id == x.Id));

            foreach (var forwardToUpdate in forwardsToUpdate)
            {
                var updatedForward = webhookForwards.First(x => x.Id == forwardToUpdate.Id);

                forwardToUpdate.FowardingDestination = updatedForward.Destination;
                forwardToUpdate.IgnoreSslErrors = updatedForward.IgnoreSslErrors;
                forwardToUpdate.ForwardGroupPreviews = updatedForward.ForwardGroupPreviews;
                forwardToUpdate.ForwardSinglePlates = updatedForward.ForwardSinglePlates;
                forwardToUpdate.ForwardGroups = updatedForward.ForwardGroups;

                _unitOfWork.WebhookForwards.Update(forwardToUpdate);
            }

            var forwardsToAdd = webhookForwards.Where(x => !dbForwards.Any(p2 => p2.Id == x.Id));

            foreach (var forwardToAdd in forwardsToAdd)
            {
                var addedForward = new Data.WebhookForward()
                {
                    FowardingDestination = forwardToAdd.Destination,
                    IgnoreSslErrors = forwardToAdd.IgnoreSslErrors,
                    ForwardGroupPreviews = forwardToAdd.ForwardGroupPreviews,
                    ForwardGroups = forwardToAdd.ForwardGroups,
                    ForwardSinglePlates = forwardToAdd.ForwardSinglePlates,
                };

                await _unitOfWork.WebhookForwards.AddAsync(addedForward, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
} 