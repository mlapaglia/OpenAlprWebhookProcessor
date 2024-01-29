using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates.GetPlate
{
    public class GetPlateHandler
    {
        private readonly ProcessorContext _processerContext;

        public GetPlateHandler(ProcessorContext processorContext)
        {
            _processerContext = processorContext;
        }

        public async Task<GetPlateResponse> HandleAsync(
            Guid plateId,
            CancellationToken cancellationToken)
        {
            var plate = await _processerContext.PlateGroups
                .AsNoTracking()
                .Include(x => x.PossibleNumbers)
                .Where(x => x.Id == plateId)
                .FirstOrDefaultAsync(cancellationToken);

            if (plate == null)
            {
                throw new ArgumentException("plate not found.");
            }

            var mappedPlate = PlateMapper.MapPlate(plate);

            return new GetPlateResponse()
            {
                Plate = mappedPlate,
            };
        }
    }
}
