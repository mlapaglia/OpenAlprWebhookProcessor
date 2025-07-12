using FluentValidation;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.UpsertPlate
{
    public class UpsertPlateCommandValidator : AbstractValidator<UpsertPlateCommand>
    {
        public UpsertPlateCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required");

            RuleFor(x => x.PlateNumber)
                .NotEmpty()
                .WithMessage("Plate number is required")
                .MaximumLength(20)
                .WithMessage("Plate number must be less than 20 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description must be less than 500 characters");

            RuleFor(x => x.VehicleColor)
                .MaximumLength(50)
                .WithMessage("Vehicle color must be less than 50 characters");

            RuleFor(x => x.VehicleMake)
                .MaximumLength(50)
                .WithMessage("Vehicle make must be less than 50 characters");

            RuleFor(x => x.VehicleModel)
                .MaximumLength(50)
                .WithMessage("Vehicle model must be less than 50 characters");

            RuleFor(x => x.VehicleType)
                .MaximumLength(50)
                .WithMessage("Vehicle type must be less than 50 characters");

            RuleFor(x => x.VehicleRegion)
                .MaximumLength(50)
                .WithMessage("Vehicle region must be less than 50 characters");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .When(x => x.Latitude.HasValue)
                .WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .When(x => x.Longitude.HasValue)
                .WithMessage("Longitude must be between -180 and 180");

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Notes must be less than 1000 characters");
        }
    }
} 