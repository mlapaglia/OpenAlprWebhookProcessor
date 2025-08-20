using Mediator;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.UpdatePlateNumber;

public class UpdatePlateNumberCommandHandler : ICommandHandler<UpdatePlateNumberCommand>
{
    private readonly ProcessorContext _context;

    public UpdatePlateNumberCommandHandler(ProcessorContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(UpdatePlateNumberCommand request, CancellationToken cancellationToken = default)
    {
        var plateGroup = await _context.PlateGroups
            .FirstOrDefaultAsync(x => x.Id == request.PlateId, cancellationToken);

        if (plateGroup == null)
        {
            throw new InvalidOperationException($"Plate with ID {request.PlateId} not found");
        }

        plateGroup.BestNumber = request.PlateNumber;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

