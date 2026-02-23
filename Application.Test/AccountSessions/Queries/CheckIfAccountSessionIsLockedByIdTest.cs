using MediatR;
using Application.Test.Constants;
using Microsoft.Extensions.Logging;
using Application.AccountSessions.Queries;

namespace Application.Test.Accounts.Queries
{
    public class CheckIfAccountSessionIsLockedByIdTest(ILogger<CheckIfAccountSessionIsLockedByIdTest> logger, ISender sender)
    {
        private readonly ISender _sender = sender;
        private readonly ILogger<CheckIfAccountSessionIsLockedByIdTest> _logger = logger;

        [Fact]
        public async Task CheckIfAccountSessionIsLockedById_Should_Return_Boolean()
        {
            var query = new CheckIfAccountSessionIsLockedByIdQuery()
            {
                AccountId = ApplicationTest.AccountId,
                MaxFailedAttempts = 5,
            };

            CancellationToken cancellationToken = default;

            var result = await _sender.Send(query, cancellationToken);

            Assert.NotNull(result);

            _logger.LogInformation(@"CheckIfAccountSessionIsLockedById_Should_Return_Boolean test passed.");
        }
    }
}
