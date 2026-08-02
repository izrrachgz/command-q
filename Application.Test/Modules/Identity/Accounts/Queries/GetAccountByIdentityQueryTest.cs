using MediatR;
using Application.Modules.Identity.Accounts.Queries;
using Application.Modules.Identity.Accounts.Commands;

namespace Application.Test.Modules.Identity.Accounts.Queries
{
    public sealed class GetAccountByIdentityQueryTest(ISender sender)
    {
        [Fact]
        public async Task GetAccountByIdentityQuery_Should_Resolve_Provisioned_Profile()
        {
            var suffix = Guid.NewGuid().ToString("N");
            var issuer = "https://identity.test/";
            var subject = $"subject-{Guid.NewGuid():N}";
            var username = $"IdentityUser{suffix}";
            var cancellationToken = TestContext.Current.CancellationToken;

            var createResult = await sender.Send(new CreateAccountCommand
            {
                Username = username,
                Email = $"{username}@test.com",
                IdentityIssuer = issuer,
                IdentitySubject = subject
            }, cancellationToken);

            var account = await sender.Send(new GetAccountByIdentityQuery
            {
                Issuer = issuer,
                Subject = subject
            }, cancellationToken);

            var differentIssuer = await sender.Send(new GetAccountByIdentityQuery
            {
                Issuer = issuer.TrimEnd('/'),
                Subject = subject
            }, cancellationToken);

            var differentSubject = await sender.Send(new GetAccountByIdentityQuery
            {
                Issuer = issuer,
                Subject = subject.ToUpperInvariant()
            }, cancellationToken);

            Assert.True(createResult.Success, createResult.Message);
            Assert.True(account.Success, account.Message);
            Assert.Equal(username, account.Detail?.Username);
            Assert.False(differentIssuer.Success);
            Assert.False(differentSubject.Success);
        }
    }
}
