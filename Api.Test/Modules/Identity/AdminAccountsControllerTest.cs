using System.Security.Claims;
using Api.Constants;
using Api.Extensions;
using Api.Modules.Identity;
using Api.Modules.Identity.Requests;
using Localization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Test.Modules.Identity
{
    public sealed class AdminAccountsControllerTest
    {
        private const string IdentityIssuer = "https://identity.test/";
        private const string ExpectedSelfRemovalMessage = "self-removal blocked";

        [Fact]
        public async Task UpdateRoles_Should_Block_Admin_When_Removing_Admin_Role_From_Self()
        {
            var accountId = Guid.NewGuid();
            var controller = CreateController(accountId);

            var result = await controller.UpdateRoles(
                accountId,
                new UpdateAccountRolesRequest { Roles = [] },
                TestContext.Current.CancellationToken);

            var conflict = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, conflict.StatusCode);

            var problem = Assert.IsType<ProblemDetails>(conflict.Value);
            Assert.Equal(ExpectedSelfRemovalMessage, problem.Detail);
            Assert.Equal(ApiProblemCodes.Accounts.RoleSelfRemoval, problem.Extensions[ApiProblemDetailsExtensions.ErrorCodeExtension]);
        }

        private static AdminAccountsController CreateController(Guid accountId)
        {
            var identity = new ClaimsIdentity(
                [
                    new Claim(AuthenticationClaimTypes.Issuer, IdentityIssuer),
                    new Claim(AuthenticationClaimTypes.Subject, accountId.ToString())
                ],
                authenticationType: "Test");

            var textProvider = new StubTextProvider();
            var services = new ServiceCollection()
                .AddSingleton<ITextProvider>(textProvider)
                .BuildServiceProvider();

            return new AdminAccountsController(null!, textProvider)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(identity),
                        RequestServices = services
                    }
                }
            };
        }

        private sealed class StubTextProvider : ITextProvider
        {
            public string Get(string key, params object[] args) => ExpectedSelfRemovalMessage;
        }
    }
}
