using FluentValidation;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("User ID must be greater than 0");

            RuleFor(x => x.FirstName)
                .MaximumLength(100)
                .WithMessage("First name must be less than 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(100)
                .WithMessage("Last name must be less than 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.LastName));

            RuleFor(x => x.Username)
                .MaximumLength(50)
                .WithMessage("Username must be less than 50 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Username));

            RuleFor(x => x.Password)
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Password));
        }
    }
} 