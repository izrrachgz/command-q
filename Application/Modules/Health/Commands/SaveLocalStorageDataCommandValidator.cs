using FluentValidation;

namespace Application.Modules.Health.Commands
{
    internal sealed class SaveLocalStorageDataCommandValidator : AbstractValidator<SaveLocalStorageDataCommand>
    {
        public SaveLocalStorageDataCommandValidator() { }
    }
}
