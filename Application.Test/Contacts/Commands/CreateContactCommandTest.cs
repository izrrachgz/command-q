using MediatR;
using Application.Test.Constants;
using Microsoft.Extensions.Logging;
using Application.Contacts.Commands;

namespace Application.Test.Contacts.Commands
{
    public sealed class CreateContactCommandTest(ILogger<CreateContactCommandTest> logger, ISender sender)
    {
        private readonly ISender _sender = sender;
        private readonly ILogger<CreateContactCommandTest> _logger = logger;

        [Fact]
        public async Task CreateContactCommand_Should_Create_Contact()
        {
            var command = new CreateContactCommand()
            {
                AccountId = TestValues.AccountId,
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
