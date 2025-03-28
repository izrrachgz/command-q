using FluentValidation;

namespace Application.HealthChecks.Queries
{
    public class CheckInternetConnectionQueryValidator : AbstractValidator<CheckInternetConnectionQuery>
    {
        public CheckInternetConnectionQueryValidator()
        {
        }
    }
}