using Common.Cqrs;
using Common.Models.Response;

namespace Application.Modules.Contacts.Commands
{
    public class CreateContactCommand : ICommand<BasicResponse>
    {
        public Guid AccountId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
