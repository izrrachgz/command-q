using MediatR;
using Application.Test.Constants;
using Microsoft.Extensions.Logging;
using Application.AccountSessions.Queries;

namespace Application.Test.Accounts.Queries
{
    public class GetAccountSessionByIdQueryTest(ILogger<GetAccountSessionByIdQueryTest> logger, ISender sender)
    {
        private readonly ISender _sender = sender;
        private readonly ILogger<GetAccountSessionByIdQueryTest> _logger = logger;

        [Fact]
        public async Task GetAccountSessionByIdQuery_Should_Return_AccountSession()
        {
            var query = new GetAccountSessionByIdQuery()
            {
                AccountId = ApplicationTest.AccountId
            };
            
            CancellationToken cancellationToken = default;
            
            var result = await _sender.Send(query, cancellationToken);
            
            Assert.NotNull(result);
            
            Assert.True(result.Success);
            
            Assert.NotNull(result.Detail);
            
            Assert.Equal(query.AccountId, result.Detail.AccountId);

            _logger.LogInformation(@"GetAccountSessionByIdQuery_Should_Return_AccountSession test passed.");
        }
    }
}
