using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProcessorContext _context;
        private IDbContextTransaction? _transaction;

        private IPlateGroupRepository? _plateGroups;
        private IAgentRepository? _agents;
        private IRepository<Alert>? _alerts;
        private IRepository<Ignore>? _ignores;
        private IRepository<Camera>? _cameras;
        private IRepository<CameraMask>? _cameraMasks;
        private IRepository<Enricher>? _enrichers;
        private IRepository<WebhookForward>? _webhookForwards;
        private IRepository<Pushover>? _pushoverAlertClients;
        private IRepository<WebPushSubscription>? _webPushSubscriptions;
        private IRepository<WebPushSettings>? _webPushSettings;

        public UnitOfWork(ProcessorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IPlateGroupRepository PlateGroups => _plateGroups ??= new PlateGroupRepository(_context);
        public IAgentRepository Agents => _agents ??= new AgentRepository(_context);
        public IRepository<Alert> Alerts => _alerts ??= new Repository<Alert>(_context);
        public IRepository<Ignore> Ignores => _ignores ??= new Repository<Ignore>(_context);
        public IRepository<Camera> Cameras => _cameras ??= new Repository<Camera>(_context);
        public IRepository<CameraMask> CameraMasks => _cameraMasks ??= new Repository<CameraMask>(_context);
        public IRepository<Enricher> Enrichers => _enrichers ??= new Repository<Enricher>(_context);
        public IRepository<WebhookForward> WebhookForwards => _webhookForwards ??= new Repository<WebhookForward>(_context);
        public IRepository<Pushover> PushoverAlertClients => _pushoverAlertClients ??= new Repository<Pushover>(_context);
        public IRepository<WebPushSubscription> WebPushSubscriptions => _webPushSubscriptions ??= new Repository<WebPushSubscription>(_context);
        public IRepository<WebPushSettings> WebPushSettings => _webPushSettings ??= new Repository<WebPushSettings>(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await SaveChangesAsync(cancellationToken);
                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
} 