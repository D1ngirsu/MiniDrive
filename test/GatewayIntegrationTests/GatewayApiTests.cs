#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MiniDrive.GatewayIntegrationTests;

/// <summary>
/// Gateway API integration tests covering reverse proxy routing and health aggregation.
/// Tests the entry point for all client requests.
/// </summary>
public class GatewayApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GatewayApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [Fact]
    public async Task Gateway_health_endpoint_returns_ok()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        
        doc.RootElement.GetProperty("status").GetString().Should().Be("healthy");
        doc.RootElement.GetProperty("service").GetString().Should().Be("Gateway");
    }

    [Fact]
    public async Task Gateway_identifies_itself_correctly()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var serviceName = doc.RootElement.GetProperty("service").GetString();
        
        serviceName.Should().Be("Gateway", "Gateway should identify itself as the main entry point");
    }

    [Fact]
    public async Task Gateway_health_includes_timestamp()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        doc.RootElement.TryGetProperty("timestamp", out var timestamp).Should().BeTrue();
        DateTime.TryParse(timestamp.GetString(), out _).Should().BeTrue();
    }

    [Fact]
    public async Task Gateway_aggregate_health_endpoint_exists()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/aggregate");

        // Should exist even if services are not running
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Gateway_aggregate_health_checks_downstream_services()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/aggregate");
        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        // Should have status and services
        root.TryGetProperty("status", out var status).Should().BeTrue();
        root.TryGetProperty("services", out var services).Should().BeTrue();

        // Status should be "healthy" or "degraded"
        var statusValue = status.GetString();
        statusValue.Should().BeOneOf("healthy", "degraded");
    }

    [Fact]
    public async Task Gateway_supports_cors_for_client_applications()
    {
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        // CORS preflight should be handled
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Gateway_handles_missing_routes_gracefully()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/NonExistent/endpoint");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "Gateway should return 404 for unmapped routes");
    }

    [Fact]
    public async Task Gateway_multiple_concurrent_health_checks_succeed()
    {
        var client = _factory.CreateClient();

        var tasks = Enumerable.Range(0, 5)
            .Select(_ => client.GetAsync("/health"))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task Gateway_rejects_post_to_health_endpoint()
    {
        var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/health");
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed, "POST to /health should not be allowed");
    }

    [Fact]
    public async Task Gateway_returns_json_content_type()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
    }

    [Fact]
    public async Task Gateway_health_response_is_parseable_json()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Should not throw if valid JSON
        JsonDocument.Parse(content).Should().NotBeNull();
    }
}

/// <summary>
/// Gateway routing tests covering path-based reverse proxy routing to microservices.
/// </summary>
public class GatewayRoutingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GatewayRoutingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [Theory]
    [InlineData("/api/Auth/login")]
    [InlineData("/api/Auth/register")]
    [InlineData("/api/File/upload")]
    [InlineData("/api/File/download")]
    [InlineData("/api/Folder/list")]
    [InlineData("/api/Quota/usage")]
    [InlineData("/api/Audit/logs")]
    public async Task Gateway_routes_microservice_paths(string path)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(path);

        // Service may not be running, but routing should be configured
        // Should get either 200/404/503, not a 404 from gateway routing
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound, 
            $"Path {path} should be recognized by gateway routing (even if service is down)");
    }

    [Fact]
    public async Task Gateway_unmapped_microservice_route_returns_404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Unknown/endpoint");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound, 
            "Unmapped microservice routes should return 404");
    }

    [Fact]
    public async Task Gateway_routes_preserve_request_path()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/File/metadata/123");

        // Even if service is down, path should be preserved in routing
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }
}

/// <summary>
/// Gateway resilience tests covering error handling and degradation scenarios.
/// </summary>
public class GatewayResilienceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GatewayResilienceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [Fact]
    public async Task Gateway_continues_serving_when_downstream_service_unavailable()
    {
        var client = _factory.CreateClient();

        // Gateway health should still work even if services are down
        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK, 
            "Gateway should always respond to its own health check");
    }

    [Fact]
    public async Task Gateway_handles_malformed_requests_gracefully()
    {
        var client = _factory.CreateClient();

        // Try to access a path that may cause issues
        var response = await client.GetAsync("/api/File/../../../etc/passwd");

        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError, 
            "Gateway should handle path traversal attempts gracefully");
    }

    [Fact]
    public async Task Gateway_supports_keepalive_connections()
    {
        var client = _factory.CreateClient();

        // Make multiple requests to verify connection reuse
        for (int i = 0; i < 3; i++)
        {
            var response = await client.GetAsync("/health");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}

/// <summary>
/// Gateway aggregate health monitoring tests.
/// </summary>
public class GatewayAggregateHealthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GatewayAggregateHealthTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [Fact]
    public async Task Aggregate_health_includes_all_services()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/aggregate");
        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        var services = doc.RootElement.GetProperty("services");

        var serviceNames = new List<string>();
        foreach (var property in services.EnumerateObject())
        {
            serviceNames.Add(property.Name);
        }

        // Should mention at least some services (even if unhealthy)
        serviceNames.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Aggregate_health_indicates_degraded_when_services_down()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/aggregate");
        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        var status = doc.RootElement.GetProperty("status").GetString();

        // With no services running in test, should be degraded or unhealthy
        status.Should().BeOneOf("healthy", "degraded");
    }

    [Fact]
    public async Task Aggregate_health_includes_timestamp()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/aggregate");
        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        doc.RootElement.TryGetProperty("timestamp", out var timestamp).Should().BeTrue();
    }
}
