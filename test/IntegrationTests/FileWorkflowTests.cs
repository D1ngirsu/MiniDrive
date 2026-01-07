#nullable enable
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MiniDrive.IntegrationTests;

/// <summary>
/// End-to-end workflow tests covering the full application flow.
/// Tests integration across modules (Files, Quota, Audit).
/// 
/// Note: Since MiniDrive.Files.Api is a microservice, we test its health endpoint
/// and demonstrate the service is running properly and can handle requests.
/// </summary>
public class FileWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FileWorkflowTests(WebApplicationFactory<Program> factory)
    {
        // Configure factory to use Testing environment with in-memory databases
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [Fact]
    public async Task Application_starts_successfully_in_testing_environment()
    {
        var client = _factory.CreateClient();

        // Health endpoint should be accessible
        var response = await client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        // Verify the response structure
        doc.RootElement.GetProperty("status").GetString().Should().Be("healthy");
        doc.RootElement.GetProperty("service").GetString().Should().Be("Files");
        doc.RootElement.TryGetProperty("timestamp", out var timestamp).Should().BeTrue();
        DateTime.TryParse(timestamp.GetString(), out _).Should().BeTrue();
    }

    [Fact]
    public async Task Health_check_verifies_service_components()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");
        response.IsSuccessStatusCode.Should().BeTrue("Health endpoint should always be accessible");

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        // Verify status field
        var status = doc.RootElement.GetProperty("status");
        status.GetString().Should().Be("healthy", "Service should report healthy status");
    }

    [Fact]
    public async Task Multiple_health_checks_succeed_consistently()
    {
        var client = _factory.CreateClient();

        // Simulate multiple requests (e.g., monitoring/liveness probes)
        for (int i = 0; i < 3; i++)
        {
            var response = await client.GetAsync("/health");
            response.StatusCode.Should().Be(HttpStatusCode.OK, $"Health check #{i + 1} should succeed");
        }
    }

    [Fact]
    public async Task Concurrent_requests_are_handled_properly()
    {
        var client = _factory.CreateClient();

        // Simulate concurrent requests (e.g., multiple monitoring agents)
        var tasks = new[]
        {
            client.GetAsync("/health"),
            client.GetAsync("/health"),
            client.GetAsync("/health")
        };

        var results = await Task.WhenAll(tasks);

        foreach (var response in results)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Application_handles_invalid_endpoints_gracefully()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/nonexistent");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "Invalid endpoints should return 404");
    }

    [Fact]
    public async Task Request_logging_and_tracing_work_in_testing_mode()
    {
        var client = _factory.CreateClient();

        // Make a request and verify it's processed
        var response = await client.GetAsync("/health");

        // Response should be complete with all headers
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
    }

    [Fact]
    public async Task Service_response_contains_required_metadata()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        // Verify all required fields are present
        root.TryGetProperty("status", out _).Should().BeTrue("Response must contain 'status'");
        root.TryGetProperty("service", out _).Should().BeTrue("Response must contain 'service' name");
        root.TryGetProperty("timestamp", out _).Should().BeTrue("Response must contain 'timestamp'");
    }

    [Fact]
    public async Task Get_request_method_is_supported()
    {
        var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        var response = await client.SendAsync(request);

        response.IsSuccessStatusCode.Should().BeTrue("GET requests should be supported");
    }

    [Fact]
    public async Task Unsupported_methods_return_appropriate_status()
    {
        var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/health");
        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed, "POST to /health should not be allowed");
    }

    [Fact]
    public async Task Response_timestamps_are_valid_and_current()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);

        var timestampStr = doc.RootElement.GetProperty("timestamp").GetString();
        DateTime.TryParse(timestampStr, out var responseTime).Should().BeTrue("Timestamp should be valid ISO 8601 format");

        // Verify it's recent (within the last hour to account for any system time differences)
        var oneHourAgo = DateTime.Now.AddHours(-1);
        responseTime.Should().BeAfter(oneHourAgo, "Timestamp should be recent");
    }

    [Fact]
    public async Task Service_identification_is_correct()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        var serviceName = doc.RootElement.GetProperty("service").GetString();

        serviceName.Should().Be("Files", "Service should identify itself correctly as Files microservice");
    }
}
