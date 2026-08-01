using FluentValidation;

namespace Application.Modules.Health.Commands
{
    public class SaveDatabaseDataCommandValidator : AbstractValidator<SaveDatabaseDataCommand>
    {
        public SaveDatabaseDataCommandValidator() { }
    }
}
