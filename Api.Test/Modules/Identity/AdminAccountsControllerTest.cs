using System.Security.Claims;
using Api.Constants;
using Api.Modules.Identity;
using Common.Models.Response;
using Localization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
                new AdminAccountsController.UpdateAccountRolesRequest { Roles = [] },
                TestContext.Current.CancellationToken);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<BasicResponse>(badRequest.Value);
            Assert.False(response.Success);
            Assert.Equal(ExpectedSelfRemovalMessage, response.Message);
        }

        private static AdminAccountsController CreateController(Guid accountId)
        {
            var identity = new ClaimsIdentity(
                [
                    new Claim(AuthenticationClaimTypes.Issuer, IdentityIssuer),
                    new Claim(AuthenticationClaimTypes.Subject, accountId.ToString())
                ],
                authenticationType: "Test");

            return new AdminAccountsController(null!, new StubTextProvider())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(identity)
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
