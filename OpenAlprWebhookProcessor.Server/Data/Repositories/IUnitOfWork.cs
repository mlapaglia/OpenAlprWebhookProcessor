using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IPlateGroupRepository PlateGroups { get; }

        IRepository<PlateGroupRaw> RawPlateGroups { get; }

        IAgentRepository Agents { get; }

        IRepository<Alert> Alerts { get; }

        IRepository<Ignore> Ignores { get; }

        IRepository<Camera> Cameras { get; }

        IRepository<CameraMask> CameraMasks { get; }

        IRepository<Enricher> Enrichers { get; }

        IRepository<WebhookForward> WebhookForwards { get; }

        IRepository<Pushover> PushoverAlertClients { get; }

        IRepository<WebPushSubscription> WebPushSubscriptions { get; }

        IRepository<WebPushSettings> WebPushSettings { get; }

        IMachineLearningConfigurationRepository MachineLearningConfigurations { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
} 