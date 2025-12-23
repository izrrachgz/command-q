using FluentValidation;
using Common.Extensions;

namespace Application.Accounts.Commands
{
    public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
    {
        public CreateAccountCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .MaximumLength(256)
                .WithMessage("Email cannot exceed 256 characters.")
                .Must(email => email.IsEmailAddress())
                .WithMessage("Email has to be a valid e-mail address.");

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required.")
                .MaximumLength(256)
                .WithMessage("Username cannot exceed 256 characters.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.")
                .MaximumLength(256)
                .WithMessage("Password cannot exceed 256 characters.");
        }
    }
}
