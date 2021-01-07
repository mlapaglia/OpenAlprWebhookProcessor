using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Cameras.Configuration;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class WebhookHandler
    {
        private readonly ILogger _logger;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly TextInfo _textInfo = CultureInfo.CurrentCulture.TextInfo;

        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        private readonly ProcessorContext _processorContext;

        private readonly AgentConfiguration _agentConfiguration;

        public WebhookHandler(
            ILogger<WebhookHandler> logger,
            IServiceScopeFactory scopeFactory,
            CameraUpdateService.CameraUpdateService cameraUpdateService,
            ProcessorContext processorContext,
            AgentConfiguration agentConfiguration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _cameraUpdateService = cameraUpdateService;
            _processorContext = processorContext;
            _agentConfiguration = agentConfiguration;
        }

        public async Task HandleWebhookAsync(Webhook webhook)
        {
            var updateRequest = new CameraUpdateRequest()
            {
                LicensePlate = webhook.Group.BestPlateNumber,
                LicensePlateJpeg = Convert.FromBase64String(webhook.Group.BestPlate.PlateCropJpeg),
                OpenAlprCameraId = webhook.Group.CameraId,
                OpenAlprProcessingTimeMs = Math.Round(webhook.Group.BestPlate.ProcessingTimeMs, 2),
                ProcessedPlateConfidence = Math.Round(webhook.Group.BestPlate.Confidence, 2),
                IsAlert = webhook.DataType == "alpr_alert",
                AlertDescription = webhook.Description,
            };

            if (webhook.Group.Vehicle != null)
            {
                updateRequest.VehicleDescription = $"{webhook.Group.Vehicle.Year[0].Name} {FormatVehicleDescription(webhook.Group.Vehicle.MakeModel[0].Name)}";
            }

            _cameraUpdateService.AddJob(updateRequest);
            _processorContext.PlateGroups.Add(new PlateGroup()
            {
                PlateNumber = webhook.Group.BestPlateNumber,
                PlateJpeg = webhook.Group.BestPlate.PlateCropJpeg,
                OpenAlprCameraId = webhook.Group.CameraId,
                OpenAlprProcessingTimeMs = Math.Round(webhook.Group.BestPlate.ProcessingTimeMs, 2),
                PlateConfidence = Math.Round(webhook.Group.BestPlate.Confidence, 2),
                IsAlert = webhook.DataType == "alpr_alert",
                AlertDescription = webhook.Description
            });

            await _processorContext.SaveChangesAsync();

            if (_agentConfiguration.OpenAlprWebServer != null
                && _agentConfiguration.OpenAlprWebServer.Endpoint != null)
            {
                await RelayWebhookAsync(webhook);
            }
        }

        private string FormatVehicleDescription(string vehicleMakeModel)
        {
            return _textInfo
                .ToTitleCase(vehicleMakeModel.Replace('_', ' '));
        }

        private async Task RelayWebhookAsync(Webhook webhook)
        {
            var clientHandler = new HttpClientHandler();

            if (_agentConfiguration.OpenAlprWebServer.IgnoreSslErrors)
            {
                clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            }

            try
            {
                await OverrideOnPremisesValuesAsync(webhook);

                var httpClient = new HttpClient(clientHandler);
                var postContent = new StringContent(JsonSerializer.Serialize(webhook));

                await httpClient.PostAsync(
                    $"{_agentConfiguration.OpenAlprWebServer.Endpoint}push",
                    postContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to relay webhook");
            }
        }

        private async Task OverrideOnPremisesValuesAsync(Webhook webhook)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var onPremCompany = await processorContext.Companies
                    .Where(x => x.Username == _agentConfiguration.OpenAlprWebServer.Username)
                    .FirstOrDefaultAsync();

                if (onPremCompany == null)
                {
                    throw new ArgumentException($"could not find company id: { webhook.Group.CompanyId } and username: { _agentConfiguration.OpenAlprWebServer.Username}");
                }

                webhook.Group.CompanyId = onPremCompany.CompanyId;
            }
        }
    }
}