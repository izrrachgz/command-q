using FluentValidation;

namespace Application.Modules.Health.Queries
{
    internal sealed class GetDatabaseDataQueryValidator : AbstractValidator<GetDatabaseDataQuery>
    {
        public GetDatabaseDataQueryValidator()
        {
        }
    }
}
