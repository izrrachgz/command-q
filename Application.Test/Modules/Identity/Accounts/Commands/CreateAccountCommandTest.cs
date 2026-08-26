using Cqrs.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Modules.Identity.Accounts.Commands;

namespace Application.Test.Modules.Identity.Accounts.Commands
{
    public sealed class CreateAccountCommandTest(ILogger<CreateAccountCommandTest> logger, IRequestDispatcher dispatcher)
    {
        private readonly IRequestDispatcher _dispatcher = dispatcher;
        private readonly ILogger<CreateAccountCommandTest> _logger = logger;

        [Fact]
        public async Task CreateAccountCommand_Should_Create_Account()
        {
            string username = $@"TestUsername{Guid.NewGuid()}";

            string email = $@"{username}@test.com";

            var command = new CreateAccountCommand()
            {
                Username = username,
                Email = email,
                IdentityIssuer = "https://identity.test",
                IdentitySubject = Guid.NewGuid().ToString()
            };

            CancellationToken cancellationToken = default;

            var result = await _dispatcher.Send(command, cancellationToken);

            Assert.NotNull(result);

            Assert.True(result.Success);

            _logger.LogInformation(@"CreateAccountCommand_Should_Create_Account test passed.");
        }
    }
}
