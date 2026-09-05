using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Abstractions;

namespace Application.Modules.Files.Commands
{
    public class UploadFileCommand : ICommand<BasicResponse>, IAccountScopedRequest
    {
        public required Guid AccountId { get; set; }

        public required string FileName { get; set; }

        public required Stream FileData { get; set; }

        public string? ContentType { get; set; }
    }
}
