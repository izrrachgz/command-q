using FluentValidation;

namespace Application.HealthChecks.Queries
{
    public class GetDatabaseDataQueryValidator : AbstractValidator<GetDatabaseDataQuery>
    {
        public GetDatabaseDataQueryValidator()
        {
        }
    }
}