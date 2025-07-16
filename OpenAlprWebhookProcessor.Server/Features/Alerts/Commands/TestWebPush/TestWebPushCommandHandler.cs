using MediatR;
using OpenAlprWebhookProcessor.Alerts.WebPush;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.TestWebPush
{
    public class TestWebPushCommandHandler : IRequestHandler<TestWebPushCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TestWebPushClientRequestHandler _testWebPushClientRequestHandler;

        public TestWebPushCommandHandler(
            IUnitOfWork unitOfWork,
            TestWebPushClientRequestHandler testWebPushClientRequestHandler)
        {
            _unitOfWork = unitOfWork;
            _testWebPushClientRequestHandler = testWebPushClientRequestHandler;
        }

        public async Task Handle(TestWebPushCommand request, CancellationToken cancellationToken)
        {
            await _testWebPushClientRequestHandler.HandleAsync(cancellationToken);
        }
    }
} 