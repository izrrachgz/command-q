using Public;
using FluentValidation;
using Application.Constants;
using Localization.Providers;

namespace Application.Modules.Files.Commands
{
    public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
    {
        public UploadFileCommandValidator(ValidationTextProvider messages)
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.Account_Id))));

            RuleFor(x => x.FileName)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.File_Name))))
                .MaximumLength(ValidationLimits.MaxLength)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Exceed), messages.Get(nameof(FieldNames.File_Name)), ValidationLimits.MaxLength));

            RuleFor(x => x.FileData)
                .NotEmpty()
                .WithMessage(messages.Get(nameof(ValidationText.Field_Is_Required), messages.Get(nameof(FieldNames.File_Data))))
                .When(x => x.FileData is null || !x.FileData.CanRead)
                .WithMessage(messages.Get(nameof(ValidationText.Field_Cannot_Be_Read), messages.Get(nameof(FieldNames.File_Data))));
        }
    }
}
