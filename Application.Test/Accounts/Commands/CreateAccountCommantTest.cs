using MediatR;
using Microsoft.Extensions.Logging;
using Application.Accounts.Commands;

namespace Application.Test.Accounts.Commands
{
    public class CreateAccountCommantTest(ILogger<CreateAccountCommantTest> logger, ISender sender)
    {
        private readonly ISender _sender = sender;
        private readonly ILogger<CreateAccountCommantTest> _logger = logger;

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
