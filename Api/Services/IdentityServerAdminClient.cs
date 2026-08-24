using System.Text.Json;
using Api.Configuration;
using System.Net.Mime;
using System.Net.Http.Json;
using Common.Models.Request;
using Common.Models.Response;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Application.Modules.Identity.Roles.Dtos;
using Application.Modules.Identity.Roles.Services;

namespace Api.Services
{
    internal sealed class IdentityServerAdminClient(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<IdentityServerAdminOptions> options) : IIdentityRoleAdministrationClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public async Task<PagedResponse<IdentityUserRoleModel>> SearchUsers(
            SearchCriteria searchCriteria,
            CancellationToken cancellationToken)
        {
            var currentOptions = GetEnabledOptions();
            using var request = CreateRequest(
                HttpMethod.Get,
                BuildUsersUri(currentOptions, searchCriteria),
                currentOptions);
            using var response = await Send(request, currentOptions, cancellationToken);

            response.EnsureSuccessStatusCode();

            var page = await response.Content.ReadFromJsonAsync<IdentityAdminPagedResponse<IdentityUserRoleModel>>(
                JsonOptions,
                cancellationToken) ?? new();

            return new(page.Collection, page.Total, page.Page, page.PageSize);
        }

        public async Task<DetailResponse<IdentityUserRoleModel>> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            var currentOptions = GetEnabledOptions();
            using var request = CreateRequest(
                HttpMethod.Get,
                BuildUri(currentOptions.BaseUrl, $"{currentOptions.UsersPath.TrimEnd('/')}/{Uri.EscapeDataString(userId.ToString())}"),
                currentOptions);
            using var response = await Send(request, currentOptions, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new(false, "Identity user was not found.", null);
            }

            response.EnsureSuccessStatusCode();

            var user = await response.Content.ReadFromJsonAsync<IdentityUserRoleModel>(JsonOptions, cancellationToken);

            return user is null
                ? new(false, "Identity user was not found.", null)
                : new(true, string.Empty, user);
        }

        public async Task<DetailResponse<IdentityRoleListModel>> GetRoles(CancellationToken cancellationToken)
        {
            var currentOptions = GetEnabledOptions();
            using var request = CreateRequest(
                HttpMethod.Get,
                BuildUri(currentOptions.BaseUrl, currentOptions.RolesPath),
                currentOptions);
            using var response = await Send(request, currentOptions, cancellationToken);

            response.EnsureSuccessStatusCode();

            var roles = await response.Content.ReadFromJsonAsync<List<IdentityRoleModel>>(JsonOptions, cancellationToken) ?? [];

            return new(true, string.Empty, new IdentityRoleListModel { Roles = roles });
        }

        public async Task<BasicResponse> UpdateUserRoles(
            Guid userId,
            IReadOnlyList<string> roles,
            CancellationToken cancellationToken)
        {
            var currentOptions = GetEnabledOptions();
            using var request = CreateRequest(
                HttpMethod.Put,
                BuildUri(currentOptions.BaseUrl, $"{currentOptions.UsersPath.TrimEnd('/')}/{Uri.EscapeDataString(userId.ToString())}/roles"),
                currentOptions);
            request.Content = JsonContent.Create(new UpdateIdentityUserRolesRequest(roles), options: JsonOptions);

            using var response = await Send(request, currentOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new(false, "Identity user roles could not be updated.");
            }

            return new(true, "Identity user roles updated.");
        }

        private static HttpRequestMessage CreateRequest(
            HttpMethod method,
            Uri uri,
            IdentityServerAdminOptions currentOptions)
        {
            var request = new HttpRequestMessage(method, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            request.Headers.TryAddWithoutValidation(currentOptions.ApiKeyHeaderName, currentOptions.ApiKey);

            return request;
        }

        private async Task<HttpResponseMessage> Send(
            HttpRequestMessage request,
            IdentityServerAdminOptions currentOptions,
            CancellationToken cancellationToken)
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(currentOptions.TimeoutSeconds);

            return await client.SendAsync(request, cancellationToken);
        }

        private IdentityServerAdminOptions GetEnabledOptions()
        {
            var currentOptions = options.CurrentValue;

            if (!currentOptions.Enabled)
            {
                throw new InvalidOperationException("IdentityServer admin client is disabled.");
            }

            return currentOptions;
        }

        private static Uri BuildUsersUri(IdentityServerAdminOptions options, SearchCriteria searchCriteria)
        {
            var criteria = searchCriteria ?? new();
            var query = new List<string>
            {
                Query("Page", Math.Max(1, criteria.Page).ToString(System.Globalization.CultureInfo.InvariantCulture)),
                Query("Size", Math.Max(1, criteria.Size).ToString(System.Globalization.CultureInfo.InvariantCulture)),
                Query("AscendingSort", criteria.AscendingSort.ToString())
            };

            AddOptionalQuery(query, "SearchTerm", criteria.SearchTerm);
            AddOptionalQuery(query, "SortBy", criteria.SortBy);
            if (criteria.StartDate.HasValue)
            {
                query.Add(Query("StartDate", FormatUtc(criteria.StartDate.Value)));
            }

            if (criteria.EndDate.HasValue)
            {
                query.Add(Query("EndDate", FormatUtc(criteria.EndDate.Value)));
            }

            var path = $"{options.UsersPath}?{string.Join('&', query)}";

            return BuildUri(options.BaseUrl, path);
        }

        private static Uri BuildUri(Uri baseUrl, string relativePath)
        {
            return new(baseUrl, relativePath.TrimStart('/'));
        }

        private static void AddOptionalQuery(List<string> query, string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                query.Add(Query(name, value.Trim()));
            }
        }

        private static string Query(string name, string value)
        {
            return $"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}";
        }

        private static string FormatUtc(DateTimeOffset value)
        {
            return value.UtcDateTime.ToString("O", System.Globalization.CultureInfo.InvariantCulture);
        }

        private sealed class IdentityAdminPagedResponse<T>
        {
            public IReadOnlyList<T> Collection { get; init; } = [];
            public long Total { get; init; }
            public int Page { get; init; } = 1;
            public int PageSize { get; init; } = 25;
        }

        private sealed record UpdateIdentityUserRolesRequest(IReadOnlyList<string> Roles);
    }
}
