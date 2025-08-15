using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Services
{
    public interface ILicensePlateFeatureExtractor
    {
        Task<List<LicensePlateTrainingData>> ExtractTrainingDataAsync(
            int batchSize = 10000,
            CancellationToken cancellationToken = default);

        Task<LicensePlateTrainingData> ExtractFeaturesForPredictionAsync(
            LicensePlateInput input,
            CancellationToken cancellationToken = default);
    }
}
