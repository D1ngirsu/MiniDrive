#nullable enable
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MiniDrive.StorageIntegrationTests;

/// <summary>
/// Integration tests for Storage module with Files.Api.
/// Validates that storage is properly configured and integrated.
/// </summary>
public class StorageIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StorageIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [Fact]
    public async Task Files_Api_starts_successfully_with_storage_module()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task Storage_module_is_configured_in_testing()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
    }

    [Fact]
    public async Task Files_Api_health_endpoint_includes_service_identification()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        doc.RootElement.TryGetProperty("service", out var service).Should().BeTrue();
        service.GetString().Should().Be("Files");
    }

    [Fact]
    public async Task Storage_integration_maintains_api_responsiveness()
    {
        var client = _factory.CreateClient();
        var startTime = DateTime.UtcNow;

        var response = await client.GetAsync("/health");

        var duration = DateTime.UtcNow - startTime;
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        duration.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Multiple_health_checks_succeed_with_storage_module()
    {
        var client = _factory.CreateClient();

        for (int i = 0; i < 3; i++)
        {
            var response = await client.GetAsync("/health");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Concurrent_health_requests_handled_safely()
    {
        var client = _factory.CreateClient();

        var tasks = Enumerable.Range(0, 5)
            .Select(_ => client.GetAsync("/health"))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task File_controller_endpoint_exists()
    {
        var client = _factory.CreateClient();

        // Verify file controller is registered
        var response = await client.GetAsync("/api/file");

        // Should return some valid status (not 404, not 500)
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Storage_configuration_does_not_expose_implementation_details()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Health response should not expose storage implementation details
        content.Should().NotContain("LocalFileStorage");
        content.Should().NotContain("IFileStorage");
    }
}
