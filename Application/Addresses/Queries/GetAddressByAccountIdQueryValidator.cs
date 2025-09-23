using FluentValidation;

namespace Application.Addresses.Queries
{
    public class GetAddressByAccountIdQueryValidator : AbstractValidator<GetAddressByAccountIdQuery>
    {
        public GetAddressByAccountIdQueryValidator()
        {
            RuleFor(q => q.AccountId)
                .GreaterThan(0);
        }
    }
}
