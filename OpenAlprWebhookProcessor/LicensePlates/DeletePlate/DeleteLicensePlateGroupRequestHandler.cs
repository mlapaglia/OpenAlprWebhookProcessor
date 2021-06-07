using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates.DeletePlate
{
    public class DeleteLicensePlateGroupRequestHandler
    {
        private readonly ProcessorContext _processerContext;

        public DeleteLicensePlateGroupRequestHandler(ProcessorContext processorContext)
        {
            _processerContext = processorContext;
        }

        public async Task HandleAsync(
            Guid plateId,
            CancellationToken cancellationToken)
        {
            var plateGroup = await _processerContext.PlateGroups
                .FirstOrDefaultAsync(x => x.Id == plateId,
                cancellationToken);

            if (plateGroup == null)
            {
                throw new ArgumentException("Unknown Plate Group Id");
            }

            _processerContext.PlateGroups.Remove(plateGroup);
            await _processerContext.SaveChangesAsync(cancellationToken);
        }
    }
}
