using Cqrs.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Test.Infrastructure;
using Application.Modules.Identity.Accounts.Queries;

namespace Application.Test.Modules.Identity.Accounts.Queries
{
    public sealed class GetAccountByUsernameQueryTest(ILogger<GetAccountByUsernameQueryTest> logger, IRequestDispatcher dispatcher)
    {
        private readonly IRequestDispatcher _dispatcher = dispatcher;
        private readonly ILogger<GetAccountByUsernameQueryTest> _logger = logger;

        [Fact]
        public async Task GetAccountByUsernameQuery_Should_Return_Account()
        {
            var account = await TestAccountFactory.Create(_dispatcher, TestContext.Current.CancellationToken);

            var query = new GetAccountByUsernameQuery()
            {
                Username = account.Username
            };

            CancellationToken cancellationToken = default;

            var result = await _dispatcher.Send(query, cancellationToken);

            Assert.NotNull(result);

            Assert.True(result.Success);

            Assert.NotNull(result.Detail);

            Assert.Equal(query.Username, result.Detail.Username);

            _logger.LogInformation(@"GetAccountByUsernameQuery_Should_Return_Account test passed.");
        }
    }
}
