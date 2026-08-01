using FluentValidation;

namespace Application.Modules.Health.Queries
{
    public class CheckInternetConnectionQueryValidator : AbstractValidator<CheckInternetConnectionQuery>
    {
        public CheckInternetConnectionQueryValidator()
        {
        }
    }
}