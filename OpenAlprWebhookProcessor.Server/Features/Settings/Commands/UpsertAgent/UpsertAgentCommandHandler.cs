using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using OpenAlprWebhookProcessor.Hydrator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertAgent
{
    public class UpsertAgentCommandHandler : ICommandHandler<UpsertAgentCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageRetrieverService _imageRetrieverService;
        private readonly IHydrationService _hydrationService;
        private readonly ISimpleCameraScheduler _cameraScheduler;

        public UpsertAgentCommandHandler(
            IUnitOfWork unitOfWork,
            IImageRetrieverService imageRetrieverService,
            IHydrationService hydrationService,
            ISimpleCameraScheduler cameraScheduler)
        {
            _unitOfWork = unitOfWork;
            _imageRetrieverService = imageRetrieverService;
            _hydrationService = hydrationService;
            _cameraScheduler = cameraScheduler;
        }

        public async ValueTask<Unit> Handle(UpsertAgentCommand request, CancellationToken cancellationToken)
        {
            var agent = request.Agent;
            var dbAgent = await _unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

            var wasImageCompressionEnabled = false;

            if (agent.IsImageCompressionEnabled && (dbAgent == null || !dbAgent.IsImageCompressionEnabled))
            {
                wasImageCompressionEnabled = true;
            }

            if (dbAgent == null)
            {
                dbAgent = new Data.Agent()
                {
                    EndpointUrl = agent.EndpointUrl,
                    IsDebugEnabled = agent.IsDebugEnabled,
                    IsImageCompressionEnabled = agent.IsImageCompressionEnabled,
                    Latitude = agent.Latitude,
                    Longitude = agent.Longitude,
                    OpenAlprWebServerUrl = agent.OpenAlprWebServerUrl,
                    ScheduledScrapingIntervalMinutes = agent.ScheduledScrapingIntervalMinutes,
                    SunriseOffset = agent.SunriseOffset,
                    SunsetOffset = agent.SunsetOffset,
                    TimeZoneOffset = agent.TimezoneOffset,
                    Uid = agent.Uid,
                };

                await _unitOfWork.Agents.AddAsync(dbAgent, cancellationToken);
            }
            else
            {
                dbAgent.EndpointUrl = agent.EndpointUrl;
                dbAgent.IsDebugEnabled = agent.IsDebugEnabled;
                dbAgent.IsImageCompressionEnabled = agent.IsImageCompressionEnabled;
                dbAgent.Latitude = agent.Latitude;
                dbAgent.Longitude = agent.Longitude;
                dbAgent.OpenAlprWebServerUrl = agent.OpenAlprWebServerUrl;
                dbAgent.ScheduledScrapingIntervalMinutes = agent.ScheduledScrapingIntervalMinutes;
                dbAgent.SunsetOffset = agent.SunsetOffset;
                dbAgent.SunriseOffset = agent.SunriseOffset;
                dbAgent.TimeZoneOffset = agent.TimezoneOffset;
                dbAgent.Uid = agent.Uid;

                _unitOfWork.Agents.Update(dbAgent);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (wasImageCompressionEnabled)
            {
                _imageRetrieverService.AddImageCompressionJob();
            }

            if (agent.ScheduledScrapingIntervalMinutes != null)
            {
                await _hydrationService.ScheduleHydrationAsync(cancellationToken);
            }

            await _cameraScheduler.RescheduleAllCamerasAsync(cancellationToken);

            return Unit.Value;
        }
    }
} 