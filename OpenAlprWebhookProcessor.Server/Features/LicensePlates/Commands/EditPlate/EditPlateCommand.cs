using Mediator;
using System;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EditPlate
{
    public class EditPlateCommand : ICommand
    {
        public Guid Id { get; set; }
        public string PlateNumber { get; set; }
        public string? Notes { get; set; }
    }
} 