using Common.Cqrs;
using Common.Models.Response;

namespace Application.Files.Commands
{
    public class UploadFileCommand : ICommand<BasicResponse>
    {
        public required string FileName { get; set; }
        public required Stream FileData { get; set; }
    }
}
