using FluentValidation;

namespace Application.HealthChecks.Commands
{
    public class SaveDatabaseDataCommandValidator : AbstractValidator<SaveDatabaseDataCommand>
    {
        public SaveDatabaseDataCommandValidator() { }
    }
}
