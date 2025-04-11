using FluentValidation;

namespace Application.Accounts.Queries
{
    public class GetAccountByUsernameQueryValidator : AbstractValidator<GetAccountByUsernameQuery>
    {
        public GetAccountByUsernameQueryValidator()
        {
            RuleFor(r => r.Username)
                .NotEmpty()
                .MinimumLength(8);

            RuleFor(r => r.Password)
                .NotEmpty()
                .MinimumLength(8);
        }
    }
}
