using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Hydrator.OpenAlprSearch;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Utilities;
using OpenAlprWebhookProcessor.Cameras.Configuration;

namespace OpenAlprWebhookProcessor.Hydrator
{
    public class HydrationService : IHostedService
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly Uri _openAlprServerUrl;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ILogger _logger;

        public HydrationService(
            IServiceScopeFactory scopeFactory,
            ILogger<HydrationService> logger,
            AgentConfiguration agentConfiguration)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _scopeFactory = scopeFactory;
            _logger = logger;
            _openAlprServerUrl = new Uri(
                Flurl.Url.Combine(
                    agentConfiguration.OpenAlprWebServerUrl,
                    "/api/search/plate",
                    $"?api_key={agentConfiguration.OpenAlprWebServerApiKey}"));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        public void StartHydration()
        {
            Task.Run(() => StartHydrationAsync());
        }

        private async Task StartHydrationAsync()
        {
            var httpClient = new HttpClient();

            var startDate = new DateTimeOffset(2020, 11, 01, 0, 0, 0, TimeSpan.Zero);

            var stopDate = DateTimeOffset.UtcNow;

            var firstRecordDate = await FindEarliestPlateGroupAsync(
                startDate,
                httpClient);

            try
            {
                var responses = new List<Response>();

                while (firstRecordDate <= stopDate)
                {
                    

                    var apiResults = await GetOpenAlprPlateGroupsFromApiAsync(
                        httpClient,
                        firstRecordDate,
                        firstRecordDate.AddDays(1));

                    responses.AddRange(apiResults);

                    _logger.LogInformation($"pulling plates from: {firstRecordDate.ToString("s")} to {firstRecordDate.AddDays(1).ToString("s")}, found {apiResults.Count} plates");

                    firstRecordDate = firstRecordDate.AddDays(1);
                }

                var plateGroups = new List<PlateGroup>();

                foreach (var response in responses)
                {
                    plateGroups.Add(MapResponseToPlate(response));
                }

                using (var scope = _scopeFactory.CreateScope())
                {
                    var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                    _logger.LogInformation($"truncating plates table");

                    await processorContext.Database.ExecuteSqlRawAsync("DELETE FROM PlateGroups;");

                    _logger.LogInformation($"populating plates table");

                    processorContext.AddRange(plateGroups);
                    await processorContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to map hydration results");
                throw;
            }
        }

        private static PlateGroup MapResponseToPlate(Response apiResponse)
        {
            var fields = apiResponse.Fields;

            var plateGroup = new PlateGroup()
            {
                Confidence = double.Parse(fields.BestConfidence),
                Direction = fields.DirectionOfTravelDegrees,
                IsAlert = false,
                Number = fields.BestPlate,
                OpenAlprCameraId = fields.CameraId,
                OpenAlprProcessingTimeMs = double.Parse(fields.ProcessingTimeMs),
                OpenAlprUuid = fields.BestUuid,
                PlateCoordinates = VehicleUtilities.FormatLicensePlateImageCoordinates(
                    new List<int>()
                    {
                        fields.PlateX1,
                        fields.PlateX2,
                        fields.PlateX3,
                        fields.PlateX4,
                    },
                    new List<int>()
                    {
                        fields.PlateY1,
                        fields.PlateY2,
                        fields.PlateY3,
                        fields.PlateY4,
                    }),
                ReceivedOnEpoch = DateTimeOffset.Parse(fields.EpochTimeStart).ToUnixTimeMilliseconds(),
                VehicleDescription = $"{VehicleUtilities.FormatVehicleDescription(fields.VehicleMakeModel)}",
            };

            return plateGroup;
        }

        private async Task<List<Response>> GetOpenAlprPlateGroupsFromApiAsync(
            HttpClient httpClient,
            DateTimeOffset dateRangeStart,
            DateTimeOffset dateRangeEnd)
        {
            var requestUrl = Flurl.Url.Combine(
                _openAlprServerUrl.ToString(),
                $"start={dateRangeStart.ToString("s", System.Globalization.CultureInfo.InvariantCulture)}",
                $"end={dateRangeEnd.ToString("s", System.Globalization.CultureInfo.InvariantCulture)}");

            var result = await httpClient.GetAsync(requestUrl);

            var response = await result.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<Response>>(response);
        }

        private async Task<DateTimeOffset> FindEarliestPlateGroupAsync(
            DateTimeOffset dateRangeStart,
            HttpClient httpClient)
        {
            var numberOfResults = 0;
            var currentRequestDate = dateRangeStart;

            _logger.LogInformation("searching for first license plate");

            try
            {
                while (numberOfResults == 0)
                {
                    _logger.LogInformation($"searching from: {currentRequestDate.ToString("s")} to {currentRequestDate.AddDays(1).ToString("s")}");

                    var requestUrl = Flurl.Url.Combine(
                        _openAlprServerUrl.ToString(),
                        $"start={currentRequestDate.ToString("s", System.Globalization.CultureInfo.InvariantCulture)}",
                        $"end={currentRequestDate.AddDays(1).ToString("s", System.Globalization.CultureInfo.InvariantCulture)}");

                    var result = await httpClient.GetAsync(requestUrl);

                    var response = await result.Content.ReadAsStringAsync();

                    numberOfResults = JsonSerializer.Deserialize<List<Response>>(response).Count;

                    currentRequestDate = currentRequestDate.AddDays(1);
                }

                return currentRequestDate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to get earliest plate date");
                throw;
            }
        }
    }
}