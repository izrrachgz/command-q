using MediatR;
using Application.Test.Constants;
using Microsoft.Extensions.Logging;
using Application.Addresses.Queries;

namespace Application.Test.Addresses.Queries
{
    public sealed class GetAddressByAccountIdQueryTest(ILogger<GetAddressByAccountIdQueryTest> logger, ISender sender)
    {
        private readonly ISender _sender = sender;
        private readonly ILogger<GetAddressByAccountIdQueryTest> _logger = logger;

        [Fact]
        public async Task GetAddressByAccountIdQuery_Should_Return_Addresses()
        {
            var query = new GetAddressByAccountIdQuery()
            {
                AccountId = TestValues.AccountId
            };

            CancellationToken cancellationToken = default;

            var result = await _sender.Send(query, cancellationToken);

            Assert.NotNull(result);

            Assert.True(result.Success);

            _logger.LogInformation(@"GetAddressByAccountIdQuery_Should_Return_Addresses test passed.");
        }
    }
}
