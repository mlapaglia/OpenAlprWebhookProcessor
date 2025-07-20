using System;
using System.IO;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Configuration
{
    /// <summary>
    /// Configuration settings and utilities for machine learning functionality.
    /// Manages model storage paths and training parameters.
    /// </summary>
    public static class MachineLearningConfiguration
    {
        public const string ModelFileName = "license-plate-prediction-model.zip";
        public const string ConfigFolderName = "config";
        public const string MlModelsFolderName = "ml-models";
        
        public static TimeSpan DefaultTrainingInterval => TimeSpan.FromHours(6);
        public static int DefaultTrainingBatchSize => 50000;
        public static int MinimumTrainingData => 100;
        public static double MinimumModelQuality => 0.05; // R-squared threshold (lowered to 5% for license plate prediction)
        
        public static string GetConfigPath()
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFolderName);
            var mlModelsPath = Path.Combine(configPath, MlModelsFolderName);
            
            Directory.CreateDirectory(configPath);
            Directory.CreateDirectory(mlModelsPath);
            
            return configPath;
        }
        
        public static string GetModelPath()
        {
            return Path.Combine(GetConfigPath(), MlModelsFolderName, ModelFileName);
        }
        
        public static string GetBackupPath(DateTime timestamp)
        {
            var configPath = GetConfigPath();
            var backupFileName = $"backup-{timestamp:yyyyMMdd-HHmmss}-{ModelFileName}";
            return Path.Combine(configPath, MlModelsFolderName, backupFileName);
        }
    }
} 