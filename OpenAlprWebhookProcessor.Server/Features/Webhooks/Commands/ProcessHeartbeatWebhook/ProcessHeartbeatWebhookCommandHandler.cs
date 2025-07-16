using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessHeartbeatWebhook
{
    public class ProcessHeartbeatWebhookCommandHandler : IRequestHandler<ProcessHeartbeatWebhookCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProcessHeartbeatWebhookCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ProcessHeartbeatWebhookCommand request, CancellationToken cancellationToken)
        {
            var agents = await _unitOfWork.Agents.GetAllAsync(cancellationToken);
            var agent = agents.FirstOrDefault();

            if (agent != null)
            {
                agent.LastHeartbeatEpochMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                _unitOfWork.Agents.Update(agent);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
} 