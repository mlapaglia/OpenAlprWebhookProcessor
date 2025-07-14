using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Hydrator;
using OpenAlprWebhookProcessor.WebhookProcessor;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertAgent
{
    public class UpsertAgentCommandHandler : IRequestHandler<UpsertAgentCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ImageRetrieverService _imageRetrieverService;
        private readonly HydrationService _hydrationService;

        public UpsertAgentCommandHandler(
            IUnitOfWork unitOfWork,
            ImageRetrieverService imageRetrieverService,
            HydrationService hydrationService)
        {
            _unitOfWork = unitOfWork;
            _imageRetrieverService = imageRetrieverService;
            _hydrationService = hydrationService;
        }

        public async Task Handle(UpsertAgentCommand request, CancellationToken cancellationToken)
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
        }
    }
} 