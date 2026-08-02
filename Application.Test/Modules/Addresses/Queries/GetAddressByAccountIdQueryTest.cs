using MediatR;
using Microsoft.Extensions.Logging;
using Application.Test.Infrastructure;
using Application.Modules.Addresses.Queries;

namespace Application.Test.Modules.Addresses.Queries
{
    public sealed class GetAddressByAccountIdQueryTest(ILogger<GetAddressByAccountIdQueryTest> logger, ISender sender)
    {
        private readonly ISender _sender = sender;
        private readonly ILogger<GetAddressByAccountIdQueryTest> _logger = logger;

        [Fact]
        public async Task GetAddressByAccountIdQuery_Should_Return_Addresses()
        {
            var account = await TestAccountFactory.Create(_sender, TestContext.Current.CancellationToken);

            var query = new GetAddressByAccountIdQuery()
            {
                AccountId = account.Id
            };

            CancellationToken cancellationToken = default;

            var result = await _sender.Send(query, cancellationToken);

            Assert.NotNull(result);

            Assert.True(result.Success);

            _logger.LogInformation(@"GetAddressByAccountIdQuery_Should_Return_Addresses test passed.");
        }
    }
}
