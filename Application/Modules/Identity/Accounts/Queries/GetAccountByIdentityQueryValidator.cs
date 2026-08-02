using FluentValidation;
using Application.Modules.Identity.Accounts;

namespace Application.Modules.Identity.Accounts.Queries
{
    public sealed class GetAccountByIdentityQueryValidator : AbstractValidator<GetAccountByIdentityQuery>
    {
        public GetAccountByIdentityQueryValidator()
        {
            RuleFor(value => value.Issuer)
                .NotEmpty()
                .MaximumLength(AccountIdentityConstants.IssuerMaxLength);

            RuleFor(value => value.Subject)
                .NotEmpty()
                .MaximumLength(AccountIdentityConstants.SubjectMaxLength);
        }
    }
}
