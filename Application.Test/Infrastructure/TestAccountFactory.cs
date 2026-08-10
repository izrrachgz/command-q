using MediatR;
using Application.Modules.Identity.Accounts.Dtos;
using Application.Modules.Identity.Accounts.Queries;
using Application.Modules.Identity.Accounts.Commands;

namespace Application.Test.Infrastructure
{
    internal static class TestAccountFactory
    {
        public static async Task<AccountModel> Create(ISender sender, CancellationToken cancellationToken = default)
        {
            var suffix = Guid.NewGuid().ToString("N");
            var username = $"TestUsername{suffix}";
            var result = await sender.Send(new CreateAccountCommand
            {
                Username = username,
                Email = $"{username}@test.com",
                IdentityIssuer = "https://identity.test",
                IdentitySubject = Guid.NewGuid().ToString()
            }, cancellationToken);

            Assert.True(result.Success, result.Message);

            var account = await sender.Send(new GetAccountByUsernameQuery
            {
                Username = username
            }, cancellationToken);

            Assert.True(account.Success, account.Message);
            Assert.NotNull(account.Detail);

            return account.Detail;
        }
    }
}
