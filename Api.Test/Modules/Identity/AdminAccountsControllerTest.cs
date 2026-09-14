using System.Security.Claims;
using Api.Constants;
using Api.Extensions;
using Api.Modules.Identity;
using Api.Modules.Identity.Requests;
using Application.Modules.Identity.Roles.Commands;
using Common.Models.Response;
using Cqrs.Abstractions;
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

        [Fact]
        public async Task Unlock_Should_Dispatch_Admin_Unlock_Command()
        {
            var currentAdminId = Guid.NewGuid();
            var lockedUserId = Guid.NewGuid();
            var dispatcher = new CapturingDispatcher();
            var controller = CreateController(currentAdminId, dispatcher);

            var result = await controller.Unlock(lockedUserId, TestContext.Current.CancellationToken);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(lockedUserId, dispatcher.UnlockedUserId);
        }

        private static AdminAccountsController CreateController(Guid accountId, IRequestDispatcher? dispatcher = null)
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

            return new AdminAccountsController(dispatcher!, textProvider)
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

        private sealed class CapturingDispatcher : IRequestDispatcher
        {
            public Guid? UnlockedUserId { get; private set; }

            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            {
                var command = Assert.IsType<UnlockIdentityUserCommand>(request);
                UnlockedUserId = command.UserId;
                return Task.FromResult((TResponse)(object)new BasicResponse(true, "Identity user unlocked."));
            }
        }
    }
}
