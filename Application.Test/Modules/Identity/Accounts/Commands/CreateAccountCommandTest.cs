using MediatR;
using Microsoft.Extensions.Logging;
using Application.Modules.Identity.Accounts.Commands;

namespace Application.Test.Modules.Identity.Accounts.Commands
{
    public sealed class CreateAccountCommandTest(ILogger<CreateAccountCommandTest> logger, ISender sender)
    {
        private readonly ISender _sender = sender;
        private readonly ILogger<CreateAccountCommandTest> _logger = logger;

        [Fact]
        public async Task CreateAccountCommand_Should_Create_Account()
        {
            string username = $@"TestUsername{Guid.NewGuid()}";

            string password = @"TestPassword123!";

            string email = $@"{username}@test.com";

            var command = new CreateAccountCommand()
            {
                Username = username,
                Email = email,
                Password = password
            };

            CancellationToken cancellationToken = default;

            var result = await _sender.Send(command, cancellationToken);

            Assert.NotNull(result);

            Assert.True(result.Success);

            _logger.LogInformation(@"CreateAccountCommand_Should_Create_Account test passed.");
        }
    }
}
