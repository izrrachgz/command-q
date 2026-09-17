using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Email.Client.Abstractions;
using Email.Client.Configuration;
using Email.Client.Exceptions;
using Email.Client.Grpc;
using Email.Client.Models;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;

namespace Email.Client.Services;

internal sealed class EmailQueueClient : IEmailQueueClient, IDisposable
{
    internal const string HttpClientName = "EmailApi";
    private readonly EmailApiClientOptions options;
    private readonly TimeProvider timeProvider;
    private readonly HttpClient httpClient;
    private readonly GrpcChannel channel;
    private readonly EmailQueue.EmailQueueClient grpcClient;
    private readonly SemaphoreSlim tokenGate = new(1, 1);
    private AccessToken? token;

    public EmailQueueClient(IHttpClientFactory httpClientFactory, IOptions<EmailApiClientOptions> options, TimeProvider timeProvider)
    {
        this.options = options.Value;
        this.timeProvider = timeProvider;
        httpClient = httpClientFactory.CreateClient(HttpClientName);
        channel = GrpcChannel.ForAddress(this.options.BaseUrl, new GrpcChannelOptions { HttpClient = httpClient, DisposeHttpClient = false });
        grpcClient = new EmailQueue.EmailQueueClient(channel);
    }

    public async Task<EmailQueueResult> EnqueueAsync(EmailQueueRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!options.Enabled) throw new InvalidOperationException("The Email API client is disabled.");
        EnqueueEmailRequest payload = await Map(request, cancellationToken);
        EnqueueEmailResponse response = await Send(payload, allowAuthenticationRetry: true, cancellationToken);
        if (!Guid.TryParse(response.Id, out Guid id) ||
            !DateTime.TryParse(response.CreatedUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime createdUtc))
            throw new InvalidOperationException("The Email API returned an invalid enqueue response.");
        return new EmailQueueResult(id, response.WasDuplicate, createdUtc.ToUniversalTime());
    }

    public async Task<EmailQueuePageResult> GetPageAsync(EmailQueuePageRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!options.Enabled) throw new InvalidOperationException("The Email API client is disabled.");
        if (string.IsNullOrWhiteSpace(request.AccountReference))
            throw new ArgumentException("An account reference is required.", nameof(request));
        if (request.Page < 1)
            throw new ArgumentOutOfRangeException(nameof(request), "The page number must be greater than zero.");
        if (request.PageSize is < 1 or > EmailQueueConstants.MaximumPageSize)
            throw new ArgumentOutOfRangeException(
                nameof(request),
                $"The page size must be between 1 and {EmailQueueConstants.MaximumPageSize}.");

        string accountReference = request.AccountReference.Trim();
        try
        {
            GetEmailPageResponse response = await Send(
                new GetEmailPageRequest
                {
                    AccountReference = accountReference,
                    Page = request.Page,
                    PageSize = request.PageSize
                },
                allowAuthenticationRetry: true,
                cancellationToken);

            EmailQueuePageResult page = Map(response);
            if (page.Page != request.Page || page.PageSize != request.PageSize ||
                page.Items.Any(item => !string.Equals(item.AccountReference, accountReference, StringComparison.Ordinal)))
                throw new InvalidOperationException("The Email API returned a page for an unexpected account or paging request.");

            return page;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new EmailApiClientException("The Email API message page request failed.", exception);
        }
    }

    public async Task<EmailQueuePageResult> SearchAsync(
        EmailQueueSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!options.Enabled) throw new InvalidOperationException("The Email API client is disabled.");
        ValidatePaging(request.Page, request.PageSize);
        ValidateFilter(request.AccountReference, nameof(request.AccountReference));
        ValidateFilter(request.SearchTerm, nameof(request.SearchTerm));
        ValidateFilter(request.Recipient, nameof(request.Recipient));
        if (request.Status == EmailDeliveryStatus.Unspecified ||
            (request.Status.HasValue && !Enum.IsDefined(request.Status.Value)))
            throw new ArgumentOutOfRangeException(nameof(request), "The email status is invalid.");
        if (request.StartUtc.HasValue && request.StartUtc.Value.Kind != DateTimeKind.Utc ||
            request.EndUtc.HasValue && request.EndUtc.Value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Email search timestamps must be UTC.", nameof(request));
        if (request.StartUtc.HasValue && request.EndUtc.HasValue && request.StartUtc > request.EndUtc)
            throw new ArgumentException("The start timestamp cannot be later than the end timestamp.", nameof(request));

        try
        {
            GetEmailPageResponse response = await Send(Map(request), allowAuthenticationRetry: true, cancellationToken);
            EmailQueuePageResult page = Map(response);
            if (page.Page != request.Page || page.PageSize != request.PageSize)
                throw new InvalidOperationException("The Email API returned a page for an unexpected paging request.");
            return page;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new EmailApiClientException("The Email API message search failed.", exception);
        }
    }

    public async Task<Models.QueuedEmailDetail?> GetDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!options.Enabled) throw new InvalidOperationException("The Email API client is disabled.");
        if (id == Guid.Empty) throw new ArgumentException("A queued email identifier is required.", nameof(id));

        try
        {
            Grpc.QueuedEmailDetail response = await Send(
                new GetEmailDetailRequest { Id = id.ToString("D") },
                allowAuthenticationRetry: true,
                cancellationToken);
            Models.QueuedEmailDetail detail = Map(response);
            if (detail.Id != id)
                throw new InvalidOperationException("The Email API returned an unexpected queued email.");
            return detail;
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new EmailApiClientException("The Email API message detail request failed.", exception);
        }
    }

    private async Task<EnqueueEmailResponse> Send(EnqueueEmailRequest request, bool allowAuthenticationRetry, CancellationToken cancellationToken)
    {
        Metadata headers = await GetHeaders(cancellationToken);
        try
        {
            return await grpcClient.EnqueueAsync(request, headers, cancellationToken: cancellationToken);
        }
        catch (RpcException exception) when (allowAuthenticationRetry && exception.StatusCode == StatusCode.Unauthenticated)
        {
            token = null;
            return await Send(request, allowAuthenticationRetry: false, cancellationToken);
        }
    }

    private async Task<GetEmailPageResponse> Send(
        GetEmailPageRequest request,
        bool allowAuthenticationRetry,
        CancellationToken cancellationToken)
    {
        Metadata headers = await GetHeaders(cancellationToken);
        try
        {
            return await grpcClient.GetPageAsync(request, headers, cancellationToken: cancellationToken);
        }
        catch (RpcException exception) when (allowAuthenticationRetry && exception.StatusCode == StatusCode.Unauthenticated)
        {
            token = null;
            return await Send(request, allowAuthenticationRetry: false, cancellationToken);
        }
    }

    private async Task<GetEmailPageResponse> Send(
        SearchEmailPageRequest request,
        bool allowAuthenticationRetry,
        CancellationToken cancellationToken)
    {
        Metadata headers = await GetHeaders(cancellationToken);
        try
        {
            return await grpcClient.SearchAsync(request, headers, cancellationToken: cancellationToken);
        }
        catch (RpcException exception) when (allowAuthenticationRetry && exception.StatusCode == StatusCode.Unauthenticated)
        {
            token = null;
            return await Send(request, allowAuthenticationRetry: false, cancellationToken);
        }
    }

    private async Task<Grpc.QueuedEmailDetail> Send(
        GetEmailDetailRequest request,
        bool allowAuthenticationRetry,
        CancellationToken cancellationToken)
    {
        Metadata headers = await GetHeaders(cancellationToken);
        try
        {
            return await grpcClient.GetDetailAsync(request, headers, cancellationToken: cancellationToken);
        }
        catch (RpcException exception) when (allowAuthenticationRetry && exception.StatusCode == StatusCode.Unauthenticated)
        {
            token = null;
            return await Send(request, allowAuthenticationRetry: false, cancellationToken);
        }
    }

    public void Dispose()
    {
        tokenGate.Dispose();
        channel.Dispose();
        httpClient.Dispose();
    }

    private async Task<AccessToken> GetAccessToken(CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        if (token is not null && token.ExpiresUtc > now.AddMinutes(1)) return token;
        await tokenGate.WaitAsync(cancellationToken);
        try
        {
            now = timeProvider.GetUtcNow();
            if (token is not null && token.ExpiresUtc > now.AddMinutes(1)) return token;
            using var request = new HttpRequestMessage(HttpMethod.Post, "auth/token")
            {
                Content = JsonContent.Create(new TokenRequest(options.UserName, options.Password))
            };
            request.Headers.TryAddWithoutValidation(options.ApiClientHeaderName, options.ApiClientKey);
            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            TokenResponse? payload = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
            token = payload is null || string.IsNullOrWhiteSpace(payload.AccessToken)
                ? throw new InvalidOperationException("The Email API returned an invalid access token.")
                : new AccessToken(payload.AccessToken, new DateTimeOffset(payload.ExpiresUtc.ToUniversalTime()));
            return token;
        }
        finally
        {
            tokenGate.Release();
        }
    }

    private async Task<Metadata> GetHeaders(CancellationToken cancellationToken)
    {
        AccessToken accessToken = await GetAccessToken(cancellationToken);
        return
        [
            new Metadata.Entry(options.ApiClientHeaderName, options.ApiClientKey),
            new Metadata.Entry("authorization", $"Bearer {accessToken.Value}")
        ];
    }

    private static async Task<EnqueueEmailRequest> Map(EmailQueueRequest source, CancellationToken cancellationToken)
    {
        var message = new EmailMessageData { Subject = source.Message.Subject };
        if (source.Message.TextBody is not null) message.TextBody = source.Message.TextBody;
        if (source.Message.HtmlBody is not null) message.HtmlBody = source.Message.HtmlBody;
        if (source.Message.From is not null) message.From = Map(source.Message.From);
        message.To.AddRange(source.Message.To.Select(Map));
        message.Cc.AddRange(source.Message.Cc.Select(Map));
        message.Bcc.AddRange(source.Message.Bcc.Select(Map));
        message.ReplyTo.AddRange(source.Message.ReplyTo.Select(Map));
        foreach (EmailAttachment attachment in source.Message.Attachments)
            message.Attachments.Add(await Map(attachment, cancellationToken));
        foreach (EmailInlineResource resource in source.Message.InlineResources)
        {
            var content = new EmailBinaryContent
            {
                FileName = resource.FileName,
                ContentId = resource.ContentId,
                Content = ByteString.CopyFrom(resource.Content.Span)
            };
            if (resource.ContentType is not null) content.ContentType = resource.ContentType;
            message.InlineResources.Add(content);
        }
        var request = new EnqueueEmailRequest
        {
            Message = message
        };
        if (source.ProfileName is not null) request.ProfileName = source.ProfileName;
        if (source.IdempotencyKey is not null) request.IdempotencyKey = source.IdempotencyKey;
        if (source.AccountReference is not null) request.AccountReference = source.AccountReference;
        if (source.CorrelationId is not null) request.CorrelationId = source.CorrelationId;
        return request;
    }

    private static EmailQueuePageResult Map(GetEmailPageResponse source)
    {
        long expectedTotalPages = source.TotalCount == 0 || source.PageSize < 1
            ? 0
            : ((source.TotalCount - 1) / source.PageSize) + 1;
        if (source.Page < 1 || source.PageSize is < 1 or > EmailQueueConstants.MaximumPageSize ||
            source.TotalCount < 0 || source.TotalPages != expectedTotalPages || source.TotalPages > int.MaxValue ||
            source.Items.Count > source.PageSize)
            throw new InvalidOperationException("The Email API returned invalid page metadata.");

        var items = new List<Models.QueuedEmailSummary>(source.Items.Count);
        foreach (Grpc.QueuedEmailSummary item in source.Items)
        {
            if (!Guid.TryParse(item.Id, out Guid id) || item.AttemptCount < 0)
                throw new InvalidOperationException("The Email API returned an invalid queued email.");

            items.Add(new Models.QueuedEmailSummary
            {
                Id = id,
                Subject = item.Subject,
                Recipients = item.Recipients.ToArray(),
                AccountReference = item.HasAccountReference ? item.AccountReference : null,
                Status = Map(item.Status),
                AttemptCount = item.AttemptCount,
                CreatedUtc = ParseUtc(item.CreatedUtc, nameof(item.CreatedUtc)),
                ModifiedUtc = ParseUtc(item.ModifiedUtc, nameof(item.ModifiedUtc)),
                SentUtc = item.HasSentUtc ? ParseUtc(item.SentUtc, nameof(item.SentUtc)) : null,
                DeadLetterUtc = item.HasDeadLetterUtc ? ParseUtc(item.DeadLetterUtc, nameof(item.DeadLetterUtc)) : null,
                LastFailureKind = item.HasLastFailureKind ? item.LastFailureKind : null,
                MessageId = item.HasMessageId ? item.MessageId : null
            });
        }

        return new EmailQueuePageResult
        {
            Items = items,
            Page = source.Page,
            PageSize = source.PageSize,
            TotalCount = source.TotalCount,
            TotalPages = source.TotalPages
        };
    }

    private static Models.QueuedEmailDetail Map(Grpc.QueuedEmailDetail source)
    {
        if (!Guid.TryParse(source.Id, out Guid id) || source.Message is null || source.AttemptCount < 0)
            throw new InvalidOperationException("The Email API returned an invalid queued email detail.");

        return new Models.QueuedEmailDetail
        {
            Id = id,
            Message = Map(source.Message),
            ProfileName = source.ProfileName,
            IdempotencyKey = source.HasIdempotencyKey ? source.IdempotencyKey : null,
            AccountReference = source.HasAccountReference ? source.AccountReference : null,
            CorrelationId = source.HasCorrelationId ? source.CorrelationId : null,
            Status = Map(source.Status),
            AttemptCount = source.AttemptCount,
            CreatedUtc = ParseUtc(source.CreatedUtc, nameof(source.CreatedUtc)),
            ModifiedUtc = ParseUtc(source.ModifiedUtc, nameof(source.ModifiedUtc)),
            NextAttemptUtc = ParseUtc(source.NextAttemptUtc, nameof(source.NextAttemptUtc)),
            SentUtc = source.HasSentUtc ? ParseUtc(source.SentUtc, nameof(source.SentUtc)) : null,
            DeadLetterUtc = source.HasDeadLetterUtc ? ParseUtc(source.DeadLetterUtc, nameof(source.DeadLetterUtc)) : null,
            LastFailureKind = source.HasLastFailureKind ? source.LastFailureKind : null,
            MessageId = source.HasMessageId ? source.MessageId : null
        };
    }

    private static SearchEmailPageRequest Map(EmailQueueSearchRequest source)
    {
        var request = new SearchEmailPageRequest
        {
            Page = source.Page,
            PageSize = source.PageSize
        };
        if (!string.IsNullOrWhiteSpace(source.AccountReference)) request.AccountReference = source.AccountReference.Trim();
        if (!string.IsNullOrWhiteSpace(source.SearchTerm)) request.SearchTerm = source.SearchTerm.Trim();
        if (!string.IsNullOrWhiteSpace(source.Recipient)) request.Recipient = source.Recipient.Trim();
        if (source.Status.HasValue) request.Status = Map(source.Status.Value);
        if (source.StartUtc.HasValue) request.StartUtc = Format(source.StartUtc.Value);
        if (source.EndUtc.HasValue) request.EndUtc = Format(source.EndUtc.Value);
        return request;
    }

    private static Email.Client.Grpc.EmailQueueStatus Map(EmailDeliveryStatus source) =>
        source switch
        {
            EmailDeliveryStatus.Pending => Email.Client.Grpc.EmailQueueStatus.Pending,
            EmailDeliveryStatus.Processing => Email.Client.Grpc.EmailQueueStatus.Processing,
            EmailDeliveryStatus.Sent => Email.Client.Grpc.EmailQueueStatus.Sent,
            EmailDeliveryStatus.DeadLetter => Email.Client.Grpc.EmailQueueStatus.DeadLetter,
            _ => throw new ArgumentOutOfRangeException(nameof(source), "The email status is invalid.")
        };

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "The page number must be greater than zero.");
        if (pageSize is < 1 or > EmailQueueConstants.MaximumPageSize)
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                $"The page size must be between 1 and {EmailQueueConstants.MaximumPageSize}.");
    }

    private static void ValidateFilter(string? value, string parameterName)
    {
        if (value?.Trim().Length > 256)
            throw new ArgumentException("The filter cannot exceed 256 characters.", parameterName);
    }

    private static string Format(DateTime value) =>
        value.ToString("O", CultureInfo.InvariantCulture);

    private static EmailMessageDetail Map(EmailMessageDetailData source)
    {
        return new EmailMessageDetail
        {
            From = source.From is null ? null : Map(source.From),
            To = source.To.Select(Map).ToArray(),
            Cc = source.Cc.Select(Map).ToArray(),
            Bcc = source.Bcc.Select(Map).ToArray(),
            ReplyTo = source.ReplyTo.Select(Map).ToArray(),
            Subject = source.Subject,
            TextBody = source.HasTextBody ? source.TextBody : null,
            HtmlBody = source.HasHtmlBody ? source.HtmlBody : null,
            Attachments = source.Attachments
                .Select(content => new EmailContentMetadata(
                    content.FileName,
                    content.HasContentType ? content.ContentType : null,
                    content.Size))
                .ToArray(),
            InlineResources = source.InlineResources
                .Select(content => new EmailContentMetadata(
                    content.FileName,
                    content.HasContentType ? content.ContentType : null,
                    content.Size,
                    content.HasContentId ? content.ContentId : null))
                .ToArray()
        };
    }

    private static EmailAddress Map(EmailAddressMessage source) =>
        new(source.Address, source.HasName ? source.Name : null);

    private static EmailDeliveryStatus Map(Email.Client.Grpc.EmailQueueStatus source) =>
        source switch
        {
            Email.Client.Grpc.EmailQueueStatus.Pending => EmailDeliveryStatus.Pending,
            Email.Client.Grpc.EmailQueueStatus.Processing => EmailDeliveryStatus.Processing,
            Email.Client.Grpc.EmailQueueStatus.Sent => EmailDeliveryStatus.Sent,
            Email.Client.Grpc.EmailQueueStatus.DeadLetter => EmailDeliveryStatus.DeadLetter,
            _ => EmailDeliveryStatus.Unspecified
        };

    private static DateTime ParseUtc(string value, string fieldName)
    {
        if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime result) ||
            result.Kind != DateTimeKind.Utc)
            throw new InvalidOperationException($"The Email API returned an invalid {fieldName} timestamp.");
        return result;
    }

    private static EmailAddressMessage Map(EmailAddress source)
    {
        var result = new EmailAddressMessage { Address = source.Address };
        if (source.Name is not null) result.Name = source.Name;
        return result;
    }

    private static async Task<EmailBinaryContent> Map(EmailAttachment source, CancellationToken cancellationToken)
    {
        byte[] content = source.Content?.ToArray() ?? await File.ReadAllBytesAsync(source.FilePath!, cancellationToken);
        var result = new EmailBinaryContent { FileName = source.FileName, Content = ByteString.CopyFrom(content) };
        if (source.ContentType is not null) result.ContentType = source.ContentType;
        return result;
    }

    private sealed record TokenRequest(string UserName, string Password);
    private sealed record TokenResponse(string AccessToken, string TokenType, DateTime ExpiresUtc, string UserName, IReadOnlyList<string> Roles);
    private sealed record AccessToken(string Value, DateTimeOffset ExpiresUtc);
}
