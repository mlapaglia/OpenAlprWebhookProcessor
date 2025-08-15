using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Data.Repositories
{
    public class MachineLearningConfigurationRepository : Repository<MachineLearningConfiguration>, IMachineLearningConfigurationRepository
    {
        public MachineLearningConfigurationRepository(ProcessorContext context) : base(context)
        {
        }

        public async Task<MachineLearningConfiguration> GetByKeyAsync(string key)
        {
            var allConfigs = await GetAllAsync();
            return allConfigs.FirstOrDefault(c => c.Key == key);
        }

        public async Task<string> GetValueAsync(string key, string defaultValue = null)
        {
            var config = await GetByKeyAsync(key);
            return config?.Value ?? defaultValue;
        }

        public async Task<int> GetIntValueAsync(string key, int defaultValue = 0)
        {
            var value = await GetValueAsync(key);
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        public async Task<double> GetDoubleValueAsync(string key, double defaultValue = 0.0)
        {
            var value = await GetValueAsync(key);
            return double.TryParse(value, out var result) ? result : defaultValue;
        }

        public async Task<TimeSpan> GetTimeSpanValueAsync(string key, TimeSpan? defaultValue = null)
        {
            var value = await GetValueAsync(key);
            return TimeSpan.TryParse(value, out var result) ? result : (defaultValue ?? TimeSpan.Zero);
        }

        public async Task<bool> GetBoolValueAsync(string key, bool defaultValue = false)
        {
            var value = await GetValueAsync(key);
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }
    }
}
