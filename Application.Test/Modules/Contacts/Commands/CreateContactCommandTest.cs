using MediatR;
using Microsoft.Extensions.Logging;
using Application.Test.Infrastructure;
using Application.Modules.Contacts.Commands;

namespace Application.Test.Modules.Contacts.Commands
{
    public sealed class CreateContactCommandTest(ILogger<CreateContactCommandTest> logger, ISender sender)
    {
        private readonly ISender _sender = sender;
        private readonly ILogger<CreateContactCommandTest> _logger = logger;

        [Fact]
        public async Task CreateContactCommand_Should_Create_Contact()
        {
            var account = await TestAccountFactory.Create(_sender, TestContext.Current.CancellationToken);

            var command = new CreateContactCommand()
            {
                AccountId = account.Id,
                Name = @"Test Contact",
            };

            CancellationToken cancellationToken = default;

            var result = await _sender.Send(command, cancellationToken);

            Assert.NotNull(result);

            Assert.True(result.Success);

            _logger.LogInformation(@"CreateContactCommand_Should_Create_Contact test passed.");
        }
    }
}
