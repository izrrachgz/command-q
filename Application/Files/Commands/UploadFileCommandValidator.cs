using Public;
using FluentValidation;
using Localization.Providers;

namespace Application.Files.Commands
{
    public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
    {
        private const int MAX_LENGTH = 256;

        public UploadFileCommandValidator(ResourceValidationTextProvider messages)
        {
            RuleFor(x => x.FileName)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), @"File Name"))
                .MaximumLength(MAX_LENGTH)
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Cannot_Exceed), @"File Name", MAX_LENGTH));

            RuleFor(x => x.FileData)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Is_Required), @"File Data"))
                .When(x => x.FileData is null || !x.FileData.CanRead)
                .WithMessage(messages.Get(nameof(ValidationMessages.Field_Cannot_Be_Read), @"File Data"));
        }
    }
}
