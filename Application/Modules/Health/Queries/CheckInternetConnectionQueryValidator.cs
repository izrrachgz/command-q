using FluentValidation;

namespace Application.Modules.Health.Queries
{
    internal sealed class CheckInternetConnectionQueryValidator : AbstractValidator<CheckInternetConnectionQuery>
    {
        public CheckInternetConnectionQueryValidator()
        {
        }
    }
}
