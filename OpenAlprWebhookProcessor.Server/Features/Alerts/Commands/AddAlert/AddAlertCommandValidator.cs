using FluentValidation;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.AddAlert
{
    public class AddAlertCommandValidator : AbstractValidator<AddAlertCommand>
    {
        public AddAlertCommandValidator()
        {
            RuleFor(x => x.Alert)
                .NotNull()
                .WithMessage("Alert is required");

            RuleFor(x => x.Alert.PlateNumber)
                .NotEmpty()
                .WithMessage("Plate number is required")
                .MaximumLength(20)
                .WithMessage("Plate number must be less than 20 characters")
                .When(x => x.Alert != null);

            RuleFor(x => x.Alert.Description)
                .MaximumLength(500)
                .WithMessage("Description must be less than 500 characters")
                .When(x => x.Alert != null && !string.IsNullOrWhiteSpace(x.Alert.Description));
        }
    }
} 