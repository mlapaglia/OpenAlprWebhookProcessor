using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates.UpsertPlate
{
    public class UpsertPlateRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public UpsertPlateRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task HandleAsync(
            LicensePlate plate,
            CancellationToken cancellationToken)
        {
            var plateToEdit = await _processorContext.PlateGroups
                .Where(x => x.Id == plate.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (plateToEdit == null)
            {
                throw new ArgumentException("Unknown plate id");
            }

            plateToEdit.BestNumber = plate.PlateNumber;

            await _processorContext.SaveChangesAsync(cancellationToken);
        }
    }
}