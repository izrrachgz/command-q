using Email.Models;
using Email.Extensions;
using System.Text.Json;
using Email.Abstractions;
using Email.Configuration;
using Microsoft.Extensions.Configuration;
using Email.IntegrationTest.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Email.IntegrationTest.Services
{
    public sealed class Smtp4DevEmailSenderTest
    {
        private static readonly Smtp4DevOptions Settings = LoadSettings();

        [Fact]
        public async Task SendAsync_Should_Deliver_A_Complete_Message_To_Smtp4Dev()
        {
            Assert.SkipUnless(
                Settings.Enabled,
                "Set Smtp4Dev__Enabled=true to run the local smtp4dev integration test.");

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddEmailServices(options => ConfigureEmail(options, Settings));

            await using var serviceProvider = services.BuildServiceProvider();
            var sender = serviceProvider.GetRequiredService<IEmailSender>();
            var marker = Guid.NewGuid().ToString("N");
            var subject = $"command-q smtp4dev integration {marker}";
            var message = new EmailMessage
            {
                To = new[] { new EmailAddress(Settings.RecipientAddress, "Integration Recipient") },
                Subject = subject,
                TextBody = "Plain-text integration body.",
                HtmlBody = "<html><body><strong>HTML integration body</strong><img src=\"cid:integration-logo\"></body></html>",
                Attachments = new[]
                {
                    EmailAttachment.FromBytes(
                        "integration.txt",
                        "smtp4dev attachment"u8.ToArray(),
                        "text/plain")
                },
                InlineResources = new[]
                {
                    EmailInlineResource.FromBytes(
                        "integration-logo.png",
                        "integration-logo",
                        new byte[] { 137, 80, 78, 71 },
                        "image/png")
                }
            };

            var result = await sender.SendAsync(message, TestContext.Current.CancellationToken);

            Assert.False(string.IsNullOrWhiteSpace(result.MessageId));
            Assert.False(string.IsNullOrWhiteSpace(result.SmtpResponse));
            Assert.Equal(1, result.MessageNumber);

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(Settings.ApiBaseUrl)
            };

            var receivedMessage = await WaitForMessage(
                httpClient,
                subject,
                TimeSpan.FromSeconds(Settings.PollingTimeoutSeconds),
                TestContext.Current.CancellationToken);

            try
            {
                Assert.Equal(subject, receivedMessage.Subject);
                Assert.False(string.IsNullOrWhiteSpace(receivedMessage.Id));
                Assert.Equal(1, receivedMessage.AttachmentCount);

                var details = await GetMessageDetails(
                    httpClient,
                    receivedMessage.Id,
                    TestContext.Current.CancellationToken);

                Assert.True(GetBoolean(details, "hasHtmlBody"));
                Assert.True(GetBoolean(details, "hasPlainTextBody"));
                Assert.True(ContainsPropertyValue(details, "contentId", "integration-logo"));
                Assert.True(ContainsPropertyValue(details, "fileName", "integration.txt"));
            }
            finally
            {
                using var deleteResponse = await httpClient.DeleteAsync(
                    $"api/Messages/{receivedMessage.Id}",
                    CancellationToken.None);
                deleteResponse.EnsureSuccessStatusCode();
            }
        }

        private static void ConfigureEmail(EmailOptions email, Smtp4DevOptions smtp4Dev)
        {
            email.Enabled = true;
            email.Smtp.Host = smtp4Dev.SmtpHost;
            email.Smtp.Port = smtp4Dev.SmtpPort;
            email.Smtp.Security = EmailSecurityMode.None;
            email.Authentication.Type = EmailAuthenticationType.Anonymous;
            email.Sender.Address = smtp4Dev.SenderAddress;
            email.Sender.Name = "command-q Integration Test";
            email.DeliveryPolicy.AllowedSenderDomains = new[] { "command-q.test" };
        }

        private static async Task<Smtp4DevMessage> WaitForMessage(
            HttpClient httpClient,
            string subject,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var deadline = DateTimeOffset.UtcNow + timeout;

            while (DateTimeOffset.UtcNow < deadline)
            {
                using var response = await httpClient.GetAsync(
                    "api/Messages?page=1&pageSize=100",
                    cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var document = await JsonDocument.ParseAsync(
                    content,
                    cancellationToken: cancellationToken);

                foreach (var item in document.RootElement.GetProperty("results").EnumerateArray())
                {
                    var itemSubject = GetString(item, "subject");

                    if (string.Equals(itemSubject, subject, StringComparison.Ordinal))
                    {
                        return new Smtp4DevMessage(
                            GetString(item, "id"),
                            itemSubject,
                            GetInt32(item, "attachmentCount"));
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
            }

            throw new TimeoutException($"smtp4dev did not expose the message '{subject}' within {timeout}.");
        }

        private static async Task<JsonElement> GetMessageDetails(
            HttpClient httpClient,
            string messageId,
            CancellationToken cancellationToken)
        {
            using var response = await httpClient.GetAsync(
                $"api/Messages/{messageId}",
                cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(
                content,
                cancellationToken: cancellationToken);

            return document.RootElement.Clone();
        }

        private static bool ContainsPropertyValue(JsonElement element, string propertyName, string expectedValue)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(property.Value.ToString(), expectedValue, StringComparison.Ordinal))
                    {
                        return true;
                    }

                    if (ContainsPropertyValue(property.Value, propertyName, expectedValue))
                    {
                        return true;
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    if (ContainsPropertyValue(item, propertyName, expectedValue))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool GetBoolean(JsonElement element, string propertyName)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    return property.Value.GetBoolean();
                }
            }

            return false;
        }

        private static int GetInt32(JsonElement element, string propertyName)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    return property.Value.GetInt32();
                }
            }

            return 0;
        }

        private static string GetString(JsonElement element, string propertyName)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    return property.Value.ToString();
                }
            }

            return string.Empty;
        }

        private static Smtp4DevOptions LoadSettings()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            return configuration
                .GetRequiredSection(Smtp4DevOptions.SectionName)
                .Get<Smtp4DevOptions>() ?? throw new InvalidOperationException(
                    "The smtp4dev integration-test settings could not be loaded.");
        }

        private sealed record Smtp4DevMessage(string Id, string Subject, int AttachmentCount);
    }
}
