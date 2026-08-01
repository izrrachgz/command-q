using FluentValidation;

namespace Application.Modules.Health.Commands
{
    public class SaveLocalStorageDataCommandValidator : AbstractValidator<SaveLocalStorageDataCommand>
    {
        public SaveLocalStorageDataCommandValidator() { }
    }
}
