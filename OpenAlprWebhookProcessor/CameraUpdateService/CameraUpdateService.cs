using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAlprWebhookProcessor.Cameras;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class CameraUpdateService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly BlockingCollection<CameraUpdateRequest> _webhooksToProcess;

        private readonly CancellationTokenSource _cancellationTokenSource;

        public CameraUpdateService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _webhooksToProcess = new BlockingCollection<CameraUpdateRequest>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void AddJob(CameraUpdateRequest request)
        {
            _webhooksToProcess.Add(request);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () => await ProcessJobsAsync());

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            _webhooksToProcess.CompleteAdding();
            _webhooksToProcess.Dispose();

            return Task.CompletedTask;
        }

        private async Task ProcessJobsAsync()
        {
            foreach (var job in _webhooksToProcess.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var camera = scope.ServiceProvider.GetRequiredService<ICamera>();

                    await camera.SetCameraTextAsync(
                        job.OpenAlprCameraId,
                        job.LicensePlate,
                        job.VehicleDescription,
                        _cancellationTokenSource.Token);
                }
            }
        }
    }
}
