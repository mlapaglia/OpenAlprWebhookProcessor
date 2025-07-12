using FluentValidation;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.DeletePlate
{
    public class DeletePlateCommandValidator : AbstractValidator<DeletePlateCommand>
    {
        public DeletePlateCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Plate ID is required");
        }
    }
} 