using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MiniDrive.IntegrationTests;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [Fact]
    public async Task Health_returns_ok_and_healthy_status()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("/health");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        doc.RootElement.GetProperty("status").GetString().Should().Be("healthy");
        doc.RootElement.GetProperty("service").GetString().Should().Be("Files");
    }

    [Fact]
    public async Task Health_returns_timestamp()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("/health");
        
        var content = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var timestamp = doc.RootElement.GetProperty("timestamp").GetString();
        timestamp.Should().NotBeNullOrEmpty();
        DateTime.TryParse(timestamp, out _).Should().BeTrue();
    }

    [Fact]
    public async Task Health_endpoint_is_accessible_via_get()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("/health");
        res.IsSuccessStatusCode.Should().BeTrue();
    }
}
