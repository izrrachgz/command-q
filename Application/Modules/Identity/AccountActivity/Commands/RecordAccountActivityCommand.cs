using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Abstractions;

namespace Application.Modules.Identity.AccountActivity.Commands
{
    public sealed class RecordAccountActivityCommand : ICommand<BasicResponse>, IAccountScopedRequest
    {
        public required Guid AccountId { get; set; }

        public required string Module { get; set; }

        public required string Action { get; set; }

        public string? EntityType { get; set; }

        public string? EntityId { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? MetadataJson { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }
    }
}
