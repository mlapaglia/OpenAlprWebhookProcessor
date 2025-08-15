using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessHeartbeatWebhook
{
    public class ProcessHeartbeatWebhookCommandHandler : ICommandHandler<ProcessHeartbeatWebhookCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProcessHeartbeatWebhookCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Unit> Handle(ProcessHeartbeatWebhookCommand request, CancellationToken cancellationToken = default)
        {
            var agent = await _unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

            if (agent != null)
            {
                agent.LastHeartbeatEpochMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                _unitOfWork.Agents.Update(agent);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            return Unit.Value;
        }
    }
} 