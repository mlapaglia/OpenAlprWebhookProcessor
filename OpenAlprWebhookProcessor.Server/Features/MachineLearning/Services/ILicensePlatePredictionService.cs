using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Services
{
    public interface ILicensePlatePredictionService
    {
        Task<LicensePlatePredictionResult> PredictNextSeenAsync(
            LicensePlateInput input,
            CancellationToken cancellationToken = default);

        Task<List<LicensePlatePredictionResult>> PredictBatchAsync(
            List<LicensePlateInput> inputs,
            CancellationToken cancellationToken = default);

        Task<List<LicensePlatePredictionResult>> GetTopPredictionsAsync(
            int topCount = 10,
            TimeSpan? withinHours = null,
            CancellationToken cancellationToken = default);

        bool IsModelAvailable();

        Task<bool> TriggerTrainingAsync();
    }
}
