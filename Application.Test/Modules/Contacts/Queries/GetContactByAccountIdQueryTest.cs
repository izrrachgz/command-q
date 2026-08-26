using Cqrs.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Test.Infrastructure;
using Application.Modules.Contacts.Queries;

namespace Application.Test.Modules.Contacts.Queries
{
    public sealed class GetContactByAccountIdQueryTest(ILogger<GetContactByAccountIdQueryTest> logger, IRequestDispatcher dispatcher)
    {
        private readonly IRequestDispatcher _dispatcher = dispatcher;
        private readonly ILogger<GetContactByAccountIdQueryTest> _logger = logger;

        [Fact]
        public async Task GetContactByAccountIdQuery_Should_Return_Contacts()
        {
            var account = await TestAccountFactory.Create(_dispatcher, TestContext.Current.CancellationToken);

            var query = new GetContactByAccountIdQuery()
            {
                AccountId = account.Id
            };

            CancellationToken cancellationToken = default;

            var result = await _dispatcher.Send(query, cancellationToken);

            Assert.NotNull(result);

            Assert.True(result.Success);

            Assert.NotNull(result.Detail);

            _logger.LogInformation(@"GetContactByAccountIdQuery_Should_Return_Contact test passed successfully.");
        }
    }
}
