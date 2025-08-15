using Microsoft.ML;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Services
{
    public interface ILicensePlateMlTrainingService
    {
        TrainingStatus GetTrainingStatus();
        
        Task<bool> TrainModelAsync();
        
        ITransformer GetCurrentModel();
        
        void LoadExistingModel();
    }
} 