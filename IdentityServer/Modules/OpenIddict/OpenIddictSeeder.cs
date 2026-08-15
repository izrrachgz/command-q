using OpenIddict.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using IdentityServer.Resources;
using IdentityServer.Modules.Accounts;
using IdentityServer.Modules.OpenIddict.Configuration;
using static OpenIddict.Abstractions.OpenIddictConstants;
using CommandQIdentityRole = IdentityServer.Modules.Accounts.IdentityRole;

namespace IdentityServer.Modules.OpenIddict
{
    internal sealed class OpenIddictSeeder(
        RoleManager<CommandQIdentityRole> roleManager,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager,
        IOptions<AuthorizationServerOptions> options)
    {
        public async Task Seed(CancellationToken cancellationToken = default)
        {
            await SeedRoles();
            await SeedApiScope(options.Value.ApiScope, cancellationToken);

            foreach (var client in options.Value.Clients)
            {
                await SeedClient(client, options.Value.ApiScope, cancellationToken);
            }
        }

        private async Task SeedRoles()
        {
            if (!await roleManager.RoleExistsAsync(AccountConstants.DefaultRole))
            {
                var result = await roleManager.CreateAsync(new CommandQIdentityRole
                {
                    Id = Guid.NewGuid(),
                    Name = AccountConstants.DefaultRole
                });

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        IdentityServerMessages.OpenIddict_DefaultIdentityRoleCreateFailed);
                }
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
