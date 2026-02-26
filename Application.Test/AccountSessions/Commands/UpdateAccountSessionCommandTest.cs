using MediatR;
using Application.Test.Constants;
using Microsoft.Extensions.Logging;
using Application.AccountSessions.Commands;

namespace Application.Test.Accounts.Commands
{
    public sealed class UpdateAccountSessionCommandTest(ILogger<UpdateAccountSessionCommandTest> logger, ISender sender)
    {
        private readonly ISender _sender = sender;
        private readonly ILogger<UpdateAccountSessionCommandTest> _logger = logger;

        [Fact]
        public async Task UpdateAccountSessionCommand_Should_Update_Session()
        {
            var command = new UpdateAccountSessionCommand
            {
                AccountId = ApplicationTest.AccountId,
                TokenVersion = 1,
                FailedLoginAttempts = 0,
                LockedUntilUtc = null
            };

            CancellationToken cancellationToken = default;

            var result = await _sender.Send(command, cancellationToken);

            Assert.NotNull(result);

            Assert.True(result.Success);

            _logger.LogInformation(@"UpdateAccountSessionCommand_Should_Update_Session passed.");
        }
    }
}
