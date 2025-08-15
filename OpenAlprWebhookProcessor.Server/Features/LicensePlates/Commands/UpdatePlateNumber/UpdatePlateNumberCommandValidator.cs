using FluentValidation;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.UpdatePlateNumber
{
    public class UpdatePlateNumberCommandValidator : AbstractValidator<UpdatePlateNumberCommand>
    {
        public UpdatePlateNumberCommandValidator()
        {
            RuleFor(x => x.PlateId)
                .NotEmpty()
                .WithMessage("Plate ID is required");

            RuleFor(x => x.PlateNumber)
                .NotEmpty()
                .WithMessage("Plate number is required")
                .MaximumLength(20)
                .WithMessage("Plate number must be less than 20 characters");
        }
    }
}
