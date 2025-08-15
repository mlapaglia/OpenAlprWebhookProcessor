using FluentValidation;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates
{
    public class SearchLicensePlatesQueryValidator : AbstractValidator<SearchLicensePlatesQuery>
    {
        public SearchLicensePlatesQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Page number must be greater than or equal to 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(1000)
                .WithMessage("Page size must be less than or equal to 1000");

            RuleFor(x => x.FilterPlatesSeenLessThan)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Filter plates seen less than must be greater than or equal to 0");

            RuleFor(x => x.StartSearchOn)
                .LessThan(x => x.EndSearchOn)
                .When(x => x.StartSearchOn.HasValue && x.EndSearchOn.HasValue)
                .WithMessage("Start date must be before end date");

            RuleFor(x => x.PlateNumber)
                .MaximumLength(20)
                .WithMessage("Plate number must be less than 20 characters");

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
        }
    }
} 