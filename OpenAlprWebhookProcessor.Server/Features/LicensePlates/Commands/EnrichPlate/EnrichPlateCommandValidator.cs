using FluentValidation;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate
{
    public class EnrichPlateCommandValidator : AbstractValidator<EnrichPlateCommand>
    {
        public EnrichPlateCommandValidator()
        {
            RuleFor(x => x.PlateId)
                .NotEmpty()
                .WithMessage("Plate ID is required");
        }
    }
} 