using MediatR;
using Application.Test.Constants;
using Microsoft.Extensions.Logging;
using Application.AccountSessions.Queries;

namespace Application.Test.AccountSessions.Queries
{
    public sealed class CheckIfAccountSessionIsLockedByIdQueryTest(ILogger<CheckIfAccountSessionIsLockedByIdQueryTest> logger, ISender sender)
    {
        private readonly ISender _sender = sender;
        private readonly ILogger<CheckIfAccountSessionIsLockedByIdQueryTest> _logger = logger;

        [Fact]
        public async Task CheckIfAccountSessionIsLockedByIdQuery_Should_Return_Boolean()
        {
            var query = new CheckIfAccountSessionIsLockedByIdQuery()
            {
                AccountId = ApplicationTest.AccountId,
                MaxFailedAttempts = 5,
            };

            CancellationToken cancellationToken = default;

            var result = await _sender.Send(query, cancellationToken);

            Assert.NotNull(result);

            _logger.LogInformation(@"CheckIfAccountSessionIsLockedByIdQuery_Should_Return_Boolean test passed.");
        }
    }
}
