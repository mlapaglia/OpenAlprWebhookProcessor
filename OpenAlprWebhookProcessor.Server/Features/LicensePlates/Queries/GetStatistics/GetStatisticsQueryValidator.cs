using FluentValidation;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetStatistics
{
    public class GetStatisticsQueryValidator : AbstractValidator<GetStatisticsQuery>
    {
        public GetStatisticsQueryValidator()
        {
            RuleFor(x => x.PlateNumber)
                .NotEmpty()
                .WithMessage("Plate number is required")
                .MaximumLength(50)
                .WithMessage("Plate number cannot exceed 50 characters");
        }
    }
} 