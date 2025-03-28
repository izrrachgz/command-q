using FluentValidation;

namespace Application.HealthChecks.Commands
{
    public class SaveLocalStorageDataCommandValidator : AbstractValidator<SaveLocalStorageDataCommand>
    {
        public SaveLocalStorageDataCommandValidator() { }
    }
}
