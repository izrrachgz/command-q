using MediatR;
using Application.Accounts.Queries;
using Microsoft.Extensions.Logging;

namespace Application.Test.Accounts.Queries.Facts
{
    public class GetAccountByUsernameQueryTest
    {
        private readonly ISender _sender;
        private readonly ILogger<GetAccountByUsernameQueryTest> _logger;

        public GetAccountByUsernameQueryTest(ILogger<GetAccountByUsernameQueryTest> logger, ISender sender)
        {
            _sender = sender;
            _logger = logger;
        }

        [Fact]
        public async Task GetAccountByUsernameQuery_Should_Return_Account()
        {

            var query = new GetAccountByUsernameQuery()
            {
                Username = @"turbodecoder",
                Password = @"encoding"
            };

            CancellationToken cancellationToken = default;

            var result = await _sender.Send(query, cancellationToken);

            Assert.NotNull(result);

            Assert.True(result.Success);

            Assert.NotNull(result.Detail);

            Assert.Equal(query.Username, result.Detail.Username);

            _logger.LogInformation(@"GetAccountByUsernameQuery test passed.");
        }
    }
}
