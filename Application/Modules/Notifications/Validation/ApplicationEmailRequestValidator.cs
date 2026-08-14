using Public;
using Email.Models;
using FluentValidation;
using Common.Extensions;
using Email.Outbox.Constants;
using Localization.Providers;
using Application.Modules.Notifications.Models;

namespace Application.Modules.Notifications.Validation
{
    internal sealed class ApplicationEmailRequestValidator : AbstractValidator<ApplicationEmailRequest>
    {
        public ApplicationEmailRequestValidator(ValidationTextProvider messages)
        {
            RuleFor(request => request.Message)
                .NotNull()
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Is_Required),
                    messages.Get(nameof(FieldNames.Email_Message))));

            When(request => request.Message is not null, () =>
            {
                RuleFor(request => request.Message.Subject)
                    .NotEmpty()
                    .WithMessage(messages.Get(
                        nameof(ValidationText.Field_Is_Required),
                        messages.Get(nameof(FieldNames.Email_Subject))))
                    .MaximumLength(NotificationsConstants.SubjectMaxLength)
                    .WithMessage(messages.Get(
                        nameof(ValidationText.Field_Cannot_Exceed),
                        messages.Get(nameof(FieldNames.Email_Subject)),
                        NotificationsConstants.SubjectMaxLength));

                RuleFor(request => request.Message)
                    .Must(HasRecipient)
                    .WithMessage(messages.Get(
                        nameof(ValidationText.Field_Is_Required),
                        messages.Get(nameof(FieldNames.Email_Recipients))))
                    .Must(HaveValidAddresses)
                    .WithMessage(messages.Get(nameof(ValidationText.Email_Has_To_Be_Valid)))
                    .Must(HasBody)
                    .WithMessage(messages.Get(
                        nameof(ValidationText.Field_Is_Required),
                        messages.Get(nameof(FieldNames.Email_Body))));
            });

            RuleFor(request => request.Metadata == null ? null : request.Metadata.IdempotencyKey)
                .MaximumLength(EmailOutboxConstants.MaxIdempotencyKeyLength)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Exceed),
                    messages.Get(nameof(FieldNames.Idempotency_Key)),
                    EmailOutboxConstants.MaxIdempotencyKeyLength));

            RuleFor(request => request.Metadata == null ? null : request.Metadata.AccountReference)
                .MaximumLength(EmailOutboxConstants.MaxReferenceLength)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Exceed),
                    messages.Get(nameof(FieldNames.Account_Reference)),
                    EmailOutboxConstants.MaxReferenceLength));

            RuleFor(request => request.Metadata == null ? null : request.Metadata.CorrelationId)
                .MaximumLength(EmailOutboxConstants.MaxReferenceLength)
                .WithMessage(messages.Get(
                    nameof(ValidationText.Field_Cannot_Exceed),
                    messages.Get(nameof(FieldNames.Correlation_Id)),
                    EmailOutboxConstants.MaxReferenceLength));

            When(request => request.ProfileName is not null, () =>
            {
                RuleFor(request => request.ProfileName)
                    .NotEmpty()
                    .MaximumLength(EmailOutboxConstants.MaxProfileNameLength);
            });
        }

        private static bool HasRecipient(EmailMessage message)
        {
            return Addresses(message.To, message.Cc, message.Bcc).Any();
        }

        private static bool HaveValidAddresses(EmailMessage message)
        {
            var addresses = Addresses(message.To, message.Cc, message.Bcc, message.ReplyTo);

            return addresses.All(address => address is not null && address.Address.IsEmailAddress()) &&
                (message.From is null || message.From.Address.IsEmailAddress());
        }

        private static bool HasBody(EmailMessage message)
        {
            return !string.IsNullOrWhiteSpace(message.TextBody) ||
                !string.IsNullOrWhiteSpace(message.HtmlBody);
        }

        private static IEnumerable<EmailAddress?> Addresses(params IReadOnlyCollection<EmailAddress>[] collections)
        {
            return collections
                .Where(collection => collection is not null)
                .SelectMany(collection => collection);
        }
    }
}
