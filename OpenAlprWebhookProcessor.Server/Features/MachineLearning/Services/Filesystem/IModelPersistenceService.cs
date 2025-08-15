using Microsoft.ML;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Services.Filesystem
{
    public interface IModelPersistenceService
    {
        Task SaveModelAsync(
            ITransformer model,
            string modelPath,
            MLContext mlContext);

        ITransformer LoadModel(
            string modelPath,
            MLContext mlContext);

        bool ModelExists(string modelPath);

        ModelFileInfo GetModelFileInfo(string modelPath);
    }
}
