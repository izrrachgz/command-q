using System.Security.Claims;
using Api.Abstractions;
using Api.Constants;
using Api.Extensions;
using Api.Modules.Identity;
using Localization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Test.Modules.Identity
{
    public sealed class AccountControllerTest
    {
        private const string IdentityIssuer = "https://identity.test/";

        [Fact]
        public async Task Me_Should_Return_Profile_Missing_When_Identity_Has_No_Application_Account()
        {
            var services = new ServiceCollection()
                .AddSingleton<ITextProvider>(new StubTextProvider())
                .BuildServiceProvider();
            var controller = new AccountController(null!, new MissingAccountAccessor())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                [
                                    new Claim(AuthenticationClaimTypes.Issuer, IdentityIssuer),
                                    new Claim(AuthenticationClaimTypes.Subject, Guid.NewGuid().ToString())
                                ],
                                authenticationType: "Test")),
                        RequestServices = services
                    }
                }
            };

            var result = await controller.Me(TestContext.Current.CancellationToken);

            var notFound = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);

            var problem = Assert.IsType<ProblemDetails>(notFound.Value);
            Assert.Equal(
                ApiProblemCodes.Accounts.ProfileMissing,
                problem.Extensions[ApiProblemDetailsExtensions.ErrorCodeExtension]);
        }

        private sealed class MissingAccountAccessor : ICurrentAccountAccessor
        {
            public Task<Api.Models.CurrentUser?> Get(CancellationToken cancellationToken = default)
            {
                return Task.FromResult<Api.Models.CurrentUser?>(null);
            }
        }

        private sealed class StubTextProvider : ITextProvider
        {
            public string Get(string key, params object[] args) => key;
        }
    }
}
