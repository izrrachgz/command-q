using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Email.Client.Abstractions;
using Email.Client.Configuration;
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

    private async Task<EnqueueEmailResponse> Send(EnqueueEmailRequest request, bool allowAuthenticationRetry, CancellationToken cancellationToken)
    {
        AccessToken accessToken = await GetAccessToken(cancellationToken);
        var headers = new Metadata
        {
            { options.ApiClientHeaderName, options.ApiClientKey },
            { "authorization", $"Bearer {accessToken.Value}" }
        };
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
