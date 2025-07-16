using MediatR;
using OpenAlprWebhookProcessor.Alerts.Pushover;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.TestPushover
{
    public class TestPushoverCommandHandler : IRequestHandler<TestPushoverCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TestPushoverClientRequestHandler _testPushoverClientRequestHandler;

        public TestPushoverCommandHandler(
            IUnitOfWork unitOfWork,
            TestPushoverClientRequestHandler testPushoverClientRequestHandler)
        {
            _unitOfWork = unitOfWork;
            _testPushoverClientRequestHandler = testPushoverClientRequestHandler;
        }

        public async Task Handle(TestPushoverCommand request, CancellationToken cancellationToken)
        {
            await _testPushoverClientRequestHandler.HandleAsync(cancellationToken);
        }
    }
} 