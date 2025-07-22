using Microsoft.ML;
using System.IO;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Services.Filesystem
{
    public class ModelPersistenceService : IModelPersistenceService
    {
        public bool ModelExists(string modelPath)
        {
            return File.Exists(modelPath);
        }

        public ModelFileInfo GetModelFileInfo(string modelPath)
        {
            if (!File.Exists(modelPath))
            {
                return new ModelFileInfo { Exists = false };
            }

            var fileInfo = new FileInfo(modelPath);
            return new ModelFileInfo
            {
                Exists = true,
                LastModified = fileInfo.LastWriteTime,
                FileSize = fileInfo.Length
            };
        }

        public async Task<ITransformer> LoadModelAsync(string modelPath, MLContext mlContext)
        {
            if (!File.Exists(modelPath))
            {
                return null;
            }

            using var fileStream = new FileStream(
                modelPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            return mlContext.Model.Load(fileStream, out var _);
        }

        public async Task SaveModelAsync(ITransformer model, string modelPath, MLContext mlContext)
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(modelPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fileStream = new FileStream(
                modelPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read);

            // Use MLContext.Model.Save with the correct parameters
            mlContext.Model.Save(model, null, fileStream);

            // Make it async for consistency (even though the operation is synchronous)
            await Task.CompletedTask;
        }
    }
}