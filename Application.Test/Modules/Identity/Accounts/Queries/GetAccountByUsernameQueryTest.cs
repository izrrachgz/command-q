using MediatR;
using Application.Test.Constants;
using Microsoft.Extensions.Logging;
using Application.Modules.Identity.Accounts.Queries;

namespace Application.Test.Modules.Identity.Accounts.Queries
{
    public sealed class GetAccountByUsernameQueryTest(ILogger<GetAccountByUsernameQueryTest> logger, ISender sender)
    {
        private readonly ISender _sender = sender;
        private readonly ILogger<GetAccountByUsernameQueryTest> _logger = logger;

        [Fact]
        public async Task GetAccountByUsernameQuery_Should_Return_Account()
        {
            var query = new GetAccountByUsernameQuery()
            {
                Username = TestValues.AccountUsername,
                Password = TestValues.AccountPassword
            };

            CancellationToken cancellationToken = default;

            var result = await _sender.Send(query, cancellationToken);

            Assert.NotNull(result);

            Assert.True(result.Success);

            Assert.NotNull(result.Detail);

            Assert.Equal(query.Username, result.Detail.Username);

            _logger.LogInformation(@"GetAccountByUsernameQuery_Should_Return_Account test passed.");
        }
    }
}
