using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.WebSockets.Queries.GetAccountInfo
{
    public class GetAccountInfoQueryHandler : IRequestHandler<GetAccountInfoQuery, AccountInfoResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAccountInfoQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AccountInfoResponse> Handle(GetAccountInfoQuery request, CancellationToken cancellationToken = default)
        {
            var agent = await _unitOfWork.Agents.GetAllAsync(cancellationToken);
            var firstAgent = agent.FirstOrDefault();

            if (firstAgent == null)
            {
                return new AccountInfoResponse();
            }

            var websocketUrl = firstAgent.OpenAlprWebServerUrl.Replace("https://", "wss://");

            return new AccountInfoResponse
            {
                WebsocketsUrl = websocketUrl + "/ws"
            };
        }
    }
} 