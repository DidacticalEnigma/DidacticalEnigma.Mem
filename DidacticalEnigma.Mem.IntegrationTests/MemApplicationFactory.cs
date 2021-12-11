using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace DidacticalEnigma.Mem.IntegrationTests
{
    public class MemApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.IntegrationTests.json",
                    optional: true,
                    reloadOnChange: false);
            });
            builder.ConfigureTestServices(ConfigureServices);
            builder.ConfigureLogging((WebHostBuilderContext context, ILoggingBuilder loggingBuilder) =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole(options => options.IncludeScopes = true);
            });
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            
        }

        private static ConcurrentDictionary<object, bool> calledFunctions =
            new ConcurrentDictionary<object, bool>();

        public async Task CallOnce<T>(Func<T, Task> function, T param)
        {
            if (calledFunctions.TryAdd(function, true))
            {
                await function(param);
            }
        }

        public void PrepareDatabase(IServiceProvider serviceProvider)
        {
            DatabaseInitializer.InitializeDb(serviceProvider);
        }
    }
}