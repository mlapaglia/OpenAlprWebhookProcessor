using Mediator;
using System;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.UpdatePlateNumber;

public class UpdatePlateNumberCommand : ICommand
{
    public Guid PlateId { get; set; }

    public string PlateNumber { get; set; }
}




















