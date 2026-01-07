using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MiniDrive.IntegrationTests;

public class ApiResponseTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiResponseTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [Fact]
    public async Task InvalidEndpoint_returns_404()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/invalid-endpoint");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HttpGet_request_returns_successful_response()
    {
        var client = _factory.CreateClient();
        
        // Using the /health endpoint which always returns 200
        var response = await client.GetAsync("/health");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
    }

    [Fact]
    public async Task Response_contains_expected_json_structure()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        doc.RootElement.TryGetProperty("status", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("service", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("timestamp", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Application_starts_without_errors()
    {
        var client = _factory.CreateClient();
        
        // Simple connectivity test - if the app crashes, this will throw
        Func<Task> act = async () => await client.GetAsync("/health");
        await act.Should().NotThrowAsync();
    }
}

public class HttpMethodTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HttpMethodTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [Fact]
    public async Task Unsupported_http_method_returns_method_not_allowed()
    {
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/health");
        var response = await client.SendAsync(request);
        
        // POST to /health should not be allowed
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Get_request_to_health_succeeds()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
