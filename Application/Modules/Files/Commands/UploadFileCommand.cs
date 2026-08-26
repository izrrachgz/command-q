using Cqrs.Abstractions;
using Common.Models.Response;

namespace Application.Modules.Files.Commands
{
    public class UploadFileCommand : ICommand<BasicResponse>
    {
        public required Guid AccountId { get; set; }

        public required string FileName { get; set; }

        public required Stream FileData { get; set; }

        public string? ContentType { get; set; }
    }
}
