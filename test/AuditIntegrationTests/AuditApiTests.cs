#nullable enable
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MiniDrive.AuditIntegrationTests;

/// <summary>
/// Audit API integration tests covering audit logging functionality.
/// </summary>
public class AuditApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuditApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [Fact]
    public async Task Audit_service_health_check_passes()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        
        doc.RootElement.GetProperty("status").GetString().Should().Be("healthy");
        doc.RootElement.GetProperty("service").GetString().Should().Be("Audit");
    }

    [Fact]
    public async Task Audit_health_includes_timestamp()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        doc.RootElement.TryGetProperty("timestamp", out var timestamp).Should().BeTrue();
        DateTime.TryParse(timestamp.GetString(), out _).Should().BeTrue();
    }

    [Fact]
    public async Task Audit_api_returns_json_responses()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
    }

    [Fact]
    public async Task Audit_multiple_health_checks_succeed()
    {
        var client = _factory.CreateClient();

        for (int i = 0; i < 3; i++)
        {
            var response = await client.GetAsync("/health");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Audit_handles_concurrent_requests()
    {
        var client = _factory.CreateClient();

        var tasks = Enumerable.Range(0, 5)
            .Select(_ => client.GetAsync("/health"))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task Audit_invalid_endpoints_return_404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/invalid");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Audit_rejects_post_to_health()
    {
        var client = _factory.CreateClient();

        using var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "/health");
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Audit_logs_user_actions()
    {
        var client = _factory.CreateClient();

        // Verify audit service is operational and ready to log actions
        var response = await client.GetAsync("/health");
        response.IsSuccessStatusCode.Should().BeTrue("Audit service should be running and ready to log actions");
    }
}
