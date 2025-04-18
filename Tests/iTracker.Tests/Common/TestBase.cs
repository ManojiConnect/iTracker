using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Configurations;
using Infrastructure.Context;
using Xunit;
using System.Net.Http;
using iTracker.Tests;

namespace iTracker.Tests.Common;

public class TestBase : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly IServiceScope Scope;
    protected readonly WebApplicationFactory<TestProgram> Factory;
    protected readonly HttpClient Client;

    protected TestBase()
    {
        Factory = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Add any test-specific service configurations here
                });
            });
        Client = Factory.CreateClient();
        ServiceProvider = Factory.Services;
        Scope = ServiceProvider.CreateScope();
    }

    protected T GetRequiredService<T>() where T : notnull
    {
        return Scope.ServiceProvider.GetRequiredService<T>();
    }

    public void Dispose()
    {
        Scope.Dispose();
        Factory.Dispose();
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
} 