using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.ProcessorHub;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.CleanupDatabase
{
    public class CleanupDatabaseCommandHandler : ICommandHandler<CleanupDatabaseCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> _processorHub;

        private readonly ILogger<CleanupDatabaseCommandHandler> _logger;

        public CleanupDatabaseCommandHandler(
            IUnitOfWork unitOfWork,
            IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> processorHub,
            ILogger<CleanupDatabaseCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _processorHub = processorHub;
            _logger = logger;
        }

        public async ValueTask<Unit> Handle(CleanupDatabaseCommand command, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting database cleanup operation");

            try
            {
                await RemoveWebhookForwardsAsync(cancellationToken);
                await RemoveWebPushSubscriptionsAsync(cancellationToken);
                await RemovePushoverClientsAsync(cancellationToken);
                await ClearImageDataAsync(cancellationToken);
                await ObfuscateLicensePlateNumbersAsync(cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Database cleanup operation completed successfully");
                await _processorHub.Clients.All.DatabaseCleanupCompleted();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database cleanup operation");
                throw;
            }

            return Unit.Value;
        }

        private async Task RemoveWebhookForwardsAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Removing all webhook forwards");
            var deletedCount = await _unitOfWork.Context.WebhookForwards.ExecuteDeleteAsync(cancellationToken);
            _logger.LogDebug("Removed {Count} webhook forward entries", deletedCount);
        }

        private async Task RemoveWebPushSubscriptionsAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Removing all web push subscriptions");

            await _unitOfWork.Context.WebPushSubscriptionKeys.ExecuteDeleteAsync(cancellationToken);

            var deletedCount = await _unitOfWork.Context.WebPushSubscriptions.ExecuteDeleteAsync(cancellationToken);
            _logger.LogDebug("Removed {Count} web push subscription entries", deletedCount);
        }

        private async Task RemovePushoverClientsAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Removing all pushover clients");
            var deletedCount = await _unitOfWork.Context.PushoverAlertClients.ExecuteDeleteAsync(cancellationToken);
            _logger.LogDebug("Removed {Count} pushover client entries", deletedCount);
        }

        private async Task ClearImageDataAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Clearing image data from PlateImage and VehicleImage tables");
            
            var plateImageUpdates = await _unitOfWork.Context.PlateImages
                .Where(x => x.Jpeg != null)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.Jpeg, (byte[])null), cancellationToken);

            var vehicleImageUpdates = await _unitOfWork.Context.VehicleImages
                .Where(x => x.Jpeg != null)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.Jpeg, (byte[])null), cancellationToken);

            _logger.LogDebug("Cleared image data from {PlateImages} PlateImage records and {VehicleImages} VehicleImage records", 
                plateImageUpdates, vehicleImageUpdates);
        }

        private async Task ObfuscateLicensePlateNumbersAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Obfuscating license plate numbers");

            var plateGroupCount = await _unitOfWork.Context.PlateGroups
                .CountAsync(x => !string.IsNullOrEmpty(x.BestNumber), cancellationToken);

            var possibleNumberCount = await _unitOfWork.Context.PlateGroupPossibleNumbers
                .CountAsync(x => !string.IsNullOrEmpty(x.Number), cancellationToken);

            await _unitOfWork.Context.Database.ExecuteSqlRawAsync(@"
                UPDATE PlateGroups 
                SET BestNumber = CASE 
                    WHEN LENGTH(BestNumber) <= 5 THEN 'XXX00'
                    WHEN LENGTH(BestNumber) <= 6 THEN 'XXX000'
                    WHEN LENGTH(BestNumber) <= 7 THEN 'XXX0000'
                    WHEN LENGTH(BestNumber) <= 8 THEN 'XXXX0000'
                    ELSE 'XXXX0000X'
                END
                WHERE BestNumber IS NOT NULL AND BestNumber != ''", cancellationToken);

            await _unitOfWork.Context.Database.ExecuteSqlRawAsync(@"
                UPDATE PlateGroupPossibleNumbers 
                SET Number = CASE 
                    WHEN LENGTH(Number) <= 5 THEN 'XXX00'
                    WHEN LENGTH(Number) <= 6 THEN 'XXX000'
                    WHEN LENGTH(Number) <= 7 THEN 'XXX0000'
                    WHEN LENGTH(Number) <= 8 THEN 'XXXX0000'
                    ELSE 'XXXX0000X'
                END
                WHERE Number IS NOT NULL AND Number != ''", cancellationToken);

            _logger.LogDebug("Obfuscated {PlateGroupCount} plate group numbers and {PossibleNumberCount} possible numbers", 
                plateGroupCount, possibleNumberCount);
        }
    }
}