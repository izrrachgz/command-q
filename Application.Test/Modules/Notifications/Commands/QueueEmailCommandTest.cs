using NodaTime;
using Email.Client.Models;
using FluentValidation;
using Email.Client.Abstractions;
using Localization.Providers;
using Application.Modules.Notifications.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Application.Modules.Notifications.Commands;
using Application.Modules.Notifications.Services;
using Application.Modules.Notifications.Validation;

namespace Application.Test.Modules.Notifications.Commands
{
    public sealed class QueueEmailCommandTest
    {
        [Fact]
        public async Task Handle_Should_Queue_Prepared_Message_With_Metadata()
        {
            Instant createdUtc = SystemClock.Instance.GetCurrentInstant();
            var expectedResult = new EmailQueueResult(Guid.NewGuid(), false, createdUtc.ToDateTimeUtc());
            var outbox = new CapturingEmailOutbox(expectedResult);
            var service = CreateService(outbox);
            var handler = new QueueEmailCommandHandler(service);
            var command = new QueueEmailCommand
            {
                Email = new ApplicationEmailRequest
                {
                    Message = CreateMessage(),
                    Metadata = new ApplicationEmailMetadata
                    {
                        IdempotencyKey = "account-created:42",
                        AccountReference = "42",
                        CorrelationId = "request-123"
                    }
                }
            };

            QueuedEmailResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            Assert.Equal(expectedResult.Id, result.Id);
            Assert.False(result.WasDuplicate);
            Assert.Equal(createdUtc, result.CreatedUtc);
            Assert.NotNull(outbox.Request);
            Assert.Same(command.Email.Message, outbox.Request.Message);
            Assert.Equal("account-created:42", outbox.Request.IdempotencyKey);
            Assert.Equal("42", outbox.Request.AccountReference);
            Assert.Equal("request-123", outbox.Request.CorrelationId);
        }

        [Fact]
        public async Task QueueHtmlAsync_Should_Build_A_Prepared_Html_Message()
        {
            var outbox = new CapturingEmailOutbox(
                new EmailQueueResult(Guid.NewGuid(), true, SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc()));
            var service = CreateService(outbox);

            QueuedEmailResult result = await service.QueueHtmlAsync(
                ["first@example.com", "second@example.com"],
                "Account report",
                "<strong>Ready</strong>",
                new ApplicationEmailMetadata { IdempotencyKey = "report:42" },
                TestContext.Current.CancellationToken);

            Assert.True(result.WasDuplicate);
            Assert.NotNull(outbox.Request);
            Assert.Equal(2, outbox.Request.Message.To.Count);
            Assert.Equal("Account report", outbox.Request.Message.Subject);
            Assert.Null(outbox.Request.Message.TextBody);
            Assert.Equal("<strong>Ready</strong>", outbox.Request.Message.HtmlBody);
            Assert.Equal("report:42", outbox.Request.IdempotencyKey);
        }

        [Fact]
        public async Task QueueAsync_Should_Reject_Invalid_Message_Before_Outbox_Write()
        {
            var outbox = new CapturingEmailOutbox(
                new EmailQueueResult(Guid.NewGuid(), false, SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc()));
            var service = CreateService(outbox);
            var message = new EmailMessage
            {
                To = [new EmailAddress("not-an-email")],
                Subject = string.Empty,
                TextBody = string.Empty
            };

            await Assert.ThrowsAsync<ValidationException>(() => service.QueueAsync(
                message,
                cancellationToken: TestContext.Current.CancellationToken));

            Assert.Null(outbox.Request);
        }

        [Fact]
        public async Task QueueAsync_Should_Reject_Named_Profile_Without_Explicit_Policy()
        {
            var outbox = new CapturingEmailOutbox(
                new EmailQueueResult(Guid.NewGuid(), false, SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc()));
            var service = CreateService(outbox);
            var request = new ApplicationEmailRequest
            {
                Message = CreateMessage(),
                ProfileName = "Support"
            };

            await Assert.ThrowsAsync<ValidationException>(() => service.QueueAsync(
                request,
                TestContext.Current.CancellationToken));

            Assert.Null(outbox.Request);
        }

        private static ApplicationEmailService CreateService(IEmailQueueClient outbox)
        {
            var validator = new ApplicationEmailRequestValidator(new ValidationTextProvider());

            return new ApplicationEmailService(
                outbox,
                validator,
                NullLogger<ApplicationEmailService>.Instance);
        }

        private static EmailMessage CreateMessage()
        {
            return new EmailMessage
            {
                To = [new EmailAddress("recipient@example.com")],
                Subject = "Welcome",
                TextBody = "Your account is ready."
            };
        }

        private sealed class CapturingEmailOutbox(EmailQueueResult result) : IEmailQueueClient
        {
            public EmailQueueRequest? Request { get; private set; }

            public Task<EmailQueueResult> EnqueueAsync(EmailQueueRequest request, CancellationToken cancellationToken = default)
            {
                Request = request;
                return Task.FromResult(result);
            }
        }
    }
}
