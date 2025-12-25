using Public;
using FluentValidation;
using Localization.Abstractions;

namespace Application.Accounts.Queries
{
    public class GetAccountByUsernameQueryValidator : AbstractValidator<GetAccountByUsernameQuery>
    {
        private const int MIN_LENGTH = 8;

        public GetAccountByUsernameQueryValidator(ITextProvider messages)
        {
            RuleFor(r => r.Username)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(Messages.Validation_Field_Is_Required), "Username"))
                .MinimumLength(MIN_LENGTH)
                .WithMessage(messages.Get(nameof(Messages.Validation_Field_Minimum_Length), "Username", MIN_LENGTH));

            RuleFor(r => r.Password)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(Messages.Validation_Field_Is_Required), "Password"))
                .MinimumLength(MIN_LENGTH)
                .WithMessage(messages.Get(nameof(Messages.Validation_Field_Minimum_Length), "Password", MIN_LENGTH));
        }
    }
}
