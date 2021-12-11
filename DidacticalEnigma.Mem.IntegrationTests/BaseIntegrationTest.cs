using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DidacticalEnigma.Mem.IntegrationTests;

public class BaseIntegrationTest : IDisposable
{
    public MemApplicationFactory WebApplicationFactory { get; }

    private readonly IServiceScope serviceScope;

    public IServiceProvider ServiceProvider => serviceScope.ServiceProvider;

    public BaseIntegrationTest()
    {
        this.WebApplicationFactory = new MemApplicationFactory();
        this.serviceScope = this.WebApplicationFactory.Services.CreateScope();
        this.WebApplicationFactory.PrepareDatabase(ServiceProvider);
    }

    public void Dispose()
    {
        this.serviceScope.Dispose();
    }
}