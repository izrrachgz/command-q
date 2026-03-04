using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.AccountSessions.Queries
{
    public class GetAccountSessionByIdQueryValidator : AbstractValidator<GetAccountSessionByIdQuery>
    {
        public GetAccountSessionByIdQueryValidator(ResourceValidationTextProvider messages)
        {
            RuleFor(r => r.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), @"Account Id"));
        }
    }
}
