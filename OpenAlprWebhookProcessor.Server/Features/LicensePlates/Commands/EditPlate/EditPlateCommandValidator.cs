using FluentValidation;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EditPlate
{
    public class EditPlateCommandValidator : AbstractValidator<EditPlateCommand>
    {
        public EditPlateCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Plate ID is required");

            RuleFor(x => x.PlateNumber)
                .NotEmpty()
                .WithMessage("Plate number is required")
                .MaximumLength(20)
                .WithMessage("Plate number must be less than 20 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Notes must be less than 1000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));
        }
    }
} 