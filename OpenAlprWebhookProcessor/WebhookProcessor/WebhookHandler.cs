using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using OpenAlprWebhookProcessor.Cameras.Configuration;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebhook;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class WebhookHandler
    {
        private readonly TextInfo _textInfo = CultureInfo.CurrentCulture.TextInfo;

        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        private readonly ProcessorContext _processorContext;

        private readonly AgentConfiguration _agentConfiguration;

        public WebhookHandler(
            CameraUpdateService.CameraUpdateService cameraUpdateService,
            ProcessorContext processorContext,
            AgentConfiguration agentConfiguration)
        {
            _cameraUpdateService = cameraUpdateService;
            _processorContext = processorContext;
            _agentConfiguration = agentConfiguration;
        }

        public async Task HandleWebhookAsync(
            string rawWebhook,
            Webhook webhook)
        {
            if (_agentConfiguration.OpenAlprWebServer != null
                && _agentConfiguration.OpenAlprWebServer.Endpoint != null)
            {
                await RelayWebhookAsync(rawWebhook);
            }

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
        }

        private string FormatVehicleDescription(string vehicleMakeModel)
        {
            return _textInfo
                .ToTitleCase(vehicleMakeModel.Replace('_', ' '));
        }

        private async Task RelayWebhookAsync(string rawWebhook)
        {
            var clientHandler = new HttpClientHandler();

            if (_agentConfiguration.OpenAlprWebServer.IgnoreSslErrors)
            {
                clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            }

            var httpClient = new HttpClient(clientHandler);
            var postContent = new StringContent(rawWebhook);

            await httpClient.PostAsync(
                $"{_agentConfiguration.OpenAlprWebServer.Endpoint}push",
                postContent);
        }
    }
}