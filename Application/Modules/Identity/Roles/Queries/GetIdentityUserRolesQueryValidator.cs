using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.Modules.Identity.Roles.Queries
{
    internal sealed class GetIdentityUserRolesQueryValidator : AbstractValidator<GetIdentityUserRolesQuery>
    {
        public GetIdentityUserRolesQueryValidator(ValidationTextProvider messages)
        {
            RuleFor(query => query.UserId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), nameof(GetIdentityUserRolesQuery.UserId)));
        }
    }
}
