using MediatR;
using Application.Test.Constants;
using Microsoft.Extensions.Logging;
using Application.Contacts.Queries;

namespace Application.Test.Contacts.Queries
{
    public class GetContactByAccountIdQueryTest(ILogger<GetContactByAccountIdQueryTest> logger, ISender sender)
    {
        private readonly ISender _sender = sender;
        private readonly ILogger<GetContactByAccountIdQueryTest> _logger = logger;

        [Fact]
        public async Task GetContactByAccountIdQuery_Should_Return_Contacts()
        {
            var query = new GetContactByAccountIdQuery()
            {
                AccountId = ApplicationTest.AccountId
            };

            CancellationToken cancellationToken = default;

            var result = await _sender.Send(query, cancellationToken);

            Assert.NotNull(result);

            Assert.True(result.Success);

            Assert.NotNull(result.Detail);

            _logger.LogInformation(@"GetContactByAccountIdQuery_Should_Return_Contact test passed successfully.");
        }
    }
}
