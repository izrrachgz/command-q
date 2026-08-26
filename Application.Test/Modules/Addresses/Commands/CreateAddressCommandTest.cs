using Cqrs.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Test.Infrastructure;
using Application.Modules.Addresses.Commands;

namespace Application.Test.Modules.Addresses.Commands
{
    public sealed class CreateAddressCommandTest(ILogger<CreateAddressCommandTest> logger, IRequestDispatcher dispatcher)
    {
        private readonly IRequestDispatcher _dispatcher = dispatcher;
        private readonly ILogger<CreateAddressCommandTest> _logger = logger;

        [Fact]
        public async Task CreateAddressCommand_Should_Create_Address()
        {
            var account = await TestAccountFactory.Create(_dispatcher, TestContext.Current.CancellationToken);

            var command = new CreateAddressCommand()
            {
                AccountId = account.Id,
                Name = @"Test Address",
                Line1 = @"123 Test Street",
                City = @"Test City",
                PostalCode = @"12345",
                Country = @"Test Country"
            };

            CancellationToken cancellationToken = default;

            var result = await _dispatcher.Send(command, cancellationToken);

            Assert.NotNull(result);

            Assert.True(result.Success);

            _logger.LogInformation(@"CreateAddressCommand_Should_Create_Address test passed.");
        }
    }
}
