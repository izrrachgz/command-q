using OpenIddict.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityServer.Localization;
using IdentityServer.Resources;
using IdentityServer.Modules.Accounts;
using IdentityServer.Modules.OpenIddict.Configuration;
using static OpenIddict.Abstractions.OpenIddictConstants;
using AppIdentityRole = IdentityServer.Modules.Accounts.IdentityRole;
using AppIdentityUser = IdentityServer.Modules.Accounts.IdentityUser;

namespace IdentityServer.Modules.OpenIddict
{
    internal sealed class OpenIddictSeeder(
        RoleManager<AppIdentityRole> roleManager,
        UserManager<AppIdentityUser> userManager,
        IHostEnvironment environment,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager,
        IOptions<AuthorizationServerOptions> options)
    {
        public async Task Seed(CancellationToken cancellationToken = default)
        {
            await SeedRoles(cancellationToken);
            await SeedApiScope(options.Value.ApiScope, cancellationToken);

            foreach (var client in options.Value.Clients)
            {
                await SeedClient(client, options.Value.ApiScope, cancellationToken);
            }
        }

        private async Task SeedRoles(CancellationToken cancellationToken)
        {
            await EnsureRole(AccountConstants.DefaultRole);
            await EnsureRole(AccountConstants.AdminRole);
            await GrantDevelopmentAdminRole(cancellationToken);
        }

        private async Task EnsureRole(string roleName)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                return;
            }

            var result = await roleManager.CreateAsync(new AppIdentityRole
            {
                Id = Guid.NewGuid(),
                Name = roleName
            });

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    IdentityServerText.Format(IdentityServerMessages.OpenIddict_IdentityRoleCreateFailed, roleName));
            }
        }

        private async Task GrantDevelopmentAdminRole(CancellationToken cancellationToken)
        {
            if (!environment.IsDevelopment())
            {
                return;
            }

            if ((await userManager.GetUsersInRoleAsync(AccountConstants.AdminRole)).Count > 0)
            {
                return;
            }

            var user = await userManager.Users
                .OrderBy(value => value.CreatedUtc)
                .ThenBy(value => value.UserName)
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return;
            }

            if (!await userManager.IsInRoleAsync(user, AccountConstants.DefaultRole))
            {
                var defaultRoleResult = await userManager.AddToRoleAsync(user, AccountConstants.DefaultRole);

                if (!defaultRoleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        IdentityServerText.Format(IdentityServerMessages.OpenIddict_IdentityRoleGrantFailed, AccountConstants.DefaultRole));
                }
            }

            var result = await userManager.AddToRoleAsync(user, AccountConstants.AdminRole);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    IdentityServerText.Format(IdentityServerMessages.OpenIddict_IdentityRoleGrantFailed, AccountConstants.AdminRole));
            }
        }

        private async Task SeedApiScope(string apiScope, CancellationToken cancellationToken)
        {
            var descriptor = new OpenIddictScopeDescriptor
            {
                Name = apiScope,
                DisplayName = IdentityServerMessages.OpenIddict_ApiScopeDisplayName,
                Resources = { apiScope }
            };

            var scope = await scopeManager.FindByNameAsync(apiScope, cancellationToken);

            if (scope is null)
            {
                await scopeManager.CreateAsync(descriptor, cancellationToken);
            }
            else
            {
                await scopeManager.UpdateAsync(scope, descriptor, cancellationToken);
            }
        }

        private async Task SeedClient(OpenIddictClientOptions client, string apiScope, CancellationToken cancellationToken)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = client.ClientId,
                ClientSecret = string.IsNullOrWhiteSpace(client.ClientSecret) ? null : client.ClientSecret,
                ClientType = string.IsNullOrWhiteSpace(client.ClientSecret)
                    ? ClientTypes.Public
                    : ClientTypes.Confidential,
                ConsentType = ConsentTypes.Implicit,
                DisplayName = client.DisplayName
            };

            descriptor.Permissions.UnionWith(
            [
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.EndSession,
                Permissions.Endpoints.Revocation,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope + apiScope
            ]);

            if (client.AllowClientCredentials)
            {
                descriptor.Permissions.Add(Permissions.GrantTypes.ClientCredentials);
            }

            descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);

            foreach (var uri in client.RedirectUris)
            {
                descriptor.RedirectUris.Add(new Uri(uri, UriKind.Absolute));
            }

            foreach (var uri in client.PostLogoutRedirectUris)
            {
                descriptor.PostLogoutRedirectUris.Add(new Uri(uri, UriKind.Absolute));
            }

            var application = await applicationManager.FindByClientIdAsync(client.ClientId, cancellationToken);

            if (application is null)
            {
                await applicationManager.CreateAsync(descriptor, cancellationToken);
            }
            else
            {
                await applicationManager.UpdateAsync(application, descriptor, cancellationToken);
            }
        }
    }
}
