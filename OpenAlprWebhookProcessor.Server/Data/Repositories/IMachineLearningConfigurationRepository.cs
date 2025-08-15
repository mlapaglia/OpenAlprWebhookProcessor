using System;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Data.Repositories
{
    public interface IMachineLearningConfigurationRepository : IRepository<MachineLearningConfiguration>
    {
        Task<MachineLearningConfiguration> GetByKeyAsync(string key);

        Task<string> GetValueAsync(string key, string defaultValue = null);

        Task<int> GetIntValueAsync(string key, int defaultValue = 0);

        Task<double> GetDoubleValueAsync(string key, double defaultValue = 0.0);

        Task<TimeSpan> GetTimeSpanValueAsync(string key, TimeSpan? defaultValue = null);

        Task<bool> GetBoolValueAsync(string key, bool defaultValue = false);
    }
}
