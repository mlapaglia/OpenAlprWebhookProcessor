using Microsoft.ML;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Services.Filesystem
{
    public class ModelPersistenceService : IModelPersistenceService
    {
        private readonly IFileSystem _fileSystem;

        public ModelPersistenceService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool ModelExists(string modelPath)
        {
            return _fileSystem.File.Exists(modelPath);
        }

        public ModelFileInfo GetModelFileInfo(string modelPath)
        {
            if (!_fileSystem.File.Exists(modelPath))
            {
                return new ModelFileInfo { Exists = false };
            }

            var fileInfo = _fileSystem.FileInfo.New(modelPath);
            return new ModelFileInfo
            {
                Exists = true,
                LastModified = fileInfo.LastWriteTime,
                FileSize = fileInfo.Length
            };
        }

        public async Task<ITransformer> LoadModelAsync(string modelPath, MLContext mlContext)
        {
            if (!_fileSystem.File.Exists(modelPath))
            {
                return null;
            }

            using var fileStream = _fileSystem.FileStream.New(
                modelPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            return mlContext.Model.Load(fileStream, out var _);
        }

        public async Task SaveModelAsync(
            ITransformer model,
            string modelPath,
            MLContext mlContext)
        {
            var directory = _fileSystem.Path.GetDirectoryName(modelPath);
            if (!_fileSystem.Directory.Exists(directory))
            {
                _fileSystem.Directory.CreateDirectory(directory);
            }

            using var fileStream = _fileSystem.FileStream.New(
                modelPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read);

            mlContext.Model.Save(model, null, fileStream);

            await Task.CompletedTask;
        }
    }
}