using FluentValidation;

namespace Application.Modules.Health.Commands
{
    internal sealed class SaveDatabaseDataCommandValidator : AbstractValidator<SaveDatabaseDataCommand>
    {
        public SaveDatabaseDataCommandValidator() { }
    }
}
