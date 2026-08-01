using FluentValidation;

namespace Application.Modules.Health.Queries
{
    public class GetDatabaseDataQueryValidator : AbstractValidator<GetDatabaseDataQuery>
    {
        public GetDatabaseDataQueryValidator()
        {
        }
    }
}