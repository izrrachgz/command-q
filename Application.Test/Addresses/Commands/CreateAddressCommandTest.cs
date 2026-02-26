using MediatR;
using Application.Test.Constants;
using Microsoft.Extensions.Logging;
using Application.Addresses.Commands;

namespace Application.Test.Addresses.Commands
{
    public sealed class CreateAddressCommandTest(ILogger<CreateAddressCommandTest> logger, ISender sender)
    {
        private readonly ISender _sender = sender;
        private readonly ILogger<CreateAddressCommandTest> _logger = logger;

        [Fact]
        public async Task CreateAddressCommand_Should_Create_Address()
        {
            var command = new CreateAddressCommand()
            {
                AccountId = ApplicationTest.AccountId,
                Name = @"Test Address",
                Line1 = @"123 Test Street",
                City = @"Test City",
                PostalCode = @"12345",
                Country = @"Test Country"
            };

            CancellationToken cancellationToken = default;

            var result = await _sender.Send(command, cancellationToken);

            Assert.NotNull(result);

            Assert.True(result.Success);

            _logger.LogInformation(@"CreateAddressCommand_Should_Create_Address test passed.");
        }
    }
}
