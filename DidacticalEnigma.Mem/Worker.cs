using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace DidacticalEnigma.Mem
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<MemContext>();

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            if (await manager.FindByClientIdAsync("DidacticalEnigma") is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "DidacticalEnigma",
                    ConsentType = ConsentTypes.Explicit,
                    DisplayName = "Didactical Enigma",
                    Type = ClientTypes.Public,
                    // set of 16 possible addresses to check
                    // workaround for not being able to set a wildcard like
                    // https://www.rfc-editor.org/rfc/rfc8252.txt describes
                    PostLogoutRedirectUris =
                    {
                        new Uri("http://127.0.0.1:62925/"),
                        new Uri("http://127.0.0.1:64186/"),
                        new Uri("http://127.0.0.1:57226/"),
                        new Uri("http://127.0.0.1:51503/"),
                        new Uri("http://127.0.0.1:64709/"),
                        new Uri("http://127.0.0.1:57190/"),
                        new Uri("http://127.0.0.1:63336/"),
                        new Uri("http://127.0.0.1:58410/"),
                        new Uri("http://127.0.0.1:61381/"),
                        new Uri("http://127.0.0.1:60796/"),
                        new Uri("http://127.0.0.1:53803/"),
                        new Uri("http://127.0.0.1:62017/"),
                        new Uri("http://127.0.0.1:61715/"),
                        new Uri("http://127.0.0.1:53264/"),
                        new Uri("http://127.0.0.1:58730/"),
                        new Uri("http://127.0.0.1:65242/"),
                    },
                    RedirectUris =
                    {
                        new Uri("http://127.0.0.1:62925/"),
                        new Uri("http://127.0.0.1:64186/"),
                        new Uri("http://127.0.0.1:57226/"),
                        new Uri("http://127.0.0.1:51503/"),
                        new Uri("http://127.0.0.1:64709/"),
                        new Uri("http://127.0.0.1:57190/"),
                        new Uri("http://127.0.0.1:63336/"),
                        new Uri("http://127.0.0.1:58410/"),
                        new Uri("http://127.0.0.1:61381/"),
                        new Uri("http://127.0.0.1:60796/"),
                        new Uri("http://127.0.0.1:53803/"),
                        new Uri("http://127.0.0.1:62017/"),
                        new Uri("http://127.0.0.1:61715/"),
                        new Uri("http://127.0.0.1:53264/"),
                        new Uri("http://127.0.0.1:58730/"),
                        new Uri("http://127.0.0.1:65242/"),
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }

        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
