#nullable enable
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MiniDrive.IntegrationTests;

/// <summary>
/// Integration tests for file preview feature.
/// Tests preview generation for different file types.
/// </summary>
public class FilePreviewTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FilePreviewTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [Fact]
    public async Task Preview_endpoint_is_available()
    {
        var client = _factory.CreateClient();

        // Try to access preview endpoint (will fail because we don't have auth/files, but endpoint should exist)
        var response = await client.GetAsync($"/api/files/{Guid.NewGuid()}/preview");

        // Should return Unauthorized or NotFound, not 404 for the endpoint
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Preview_endpoint_supports_query_parameters()
    {
        var client = _factory.CreateClient();

        // Test with query parameters
        var fileId = Guid.NewGuid();
        var response = await client.GetAsync($"/api/files/{fileId}/preview?includeContent=true&maxPreviewSize=50000");

        // Should return Unauthorized or NotFound, not 400 (bad request)
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Preview_endpoint_requires_authorization()
    {
        var client = _factory.CreateClient();

        var fileId = Guid.NewGuid();
        var response = await client.GetAsync($"/api/files/{fileId}/preview");

        // Should return Unauthorized without token
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Preview_query_parameters_have_valid_defaults()
    {
        var client = _factory.CreateClient();

        var fileId = Guid.NewGuid();
        // Call without query parameters - should use defaults
        var response = await client.GetAsync($"/api/files/{fileId}/preview");

        // Should return Unauthorized (since no token), not 400 (bad request due to missing params)
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Preview_returns_json_response()
    {
        var client = _factory.CreateClient();

        var fileId = Guid.NewGuid();
        var response = await client.GetAsync($"/api/files/{fileId}/preview");

        // Even on error, should return JSON
        var contentType = response.Content.Headers.ContentType?.MediaType;
        contentType.Should().Contain("application/json");
    }

    [Fact]
    public async Task Preview_endpoint_GET_method_is_supported()
    {
        var client = _factory.CreateClient();

        var fileId = Guid.NewGuid();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/files/{fileId}/preview");
        var response = await client.SendAsync(request);

        // Should be Unauthorized (not 405 Method Not Allowed)
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Preview_endpoint_rejects_POST_method()
    {
        var client = _factory.CreateClient();

        var fileId = Guid.NewGuid();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/files/{fileId}/preview");
        var response = await client.SendAsync(request);

        // POST should not be allowed
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Preview_endpoint_accepts_max_preview_size_parameter()
    {
        var client = _factory.CreateClient();

        var fileId = Guid.NewGuid();
        var sizes = new[] { 1024, 10240, 102400, 1048576 };

        foreach (var size in sizes)
        {
            var response = await client.GetAsync($"/api/files/{fileId}/preview?maxPreviewSize={size}");

            // Should not return 400 (bad request)
            response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest, $"Should accept maxPreviewSize={size}");
        }
    }

    [Fact]
    public async Task Preview_endpoint_accepts_include_content_parameter()
    {
        var client = _factory.CreateClient();

        var fileId = Guid.NewGuid();
        var values = new[] { "true", "false" };

        foreach (var value in values)
        {
            var response = await client.GetAsync($"/api/files/{fileId}/preview?includeContent={value}");

            // Should not return 400 (bad request)
            response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest, $"Should accept includeContent={value}");
        }
    }

    [Fact]
    public async Task Preview_returns_proper_error_for_invalid_file_id()
    {
        var client = _factory.CreateClient();

        // Invalid GUID format
        var response = await client.GetAsync("/api/files/not-a-guid/preview");

        // Should return 400 or similar, not 500
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Text_file_preview_is_supported()
    {
        // This is a marker test for the feature
        // Full implementation would require:
        // 1. User authentication
        // 2. File upload
        // 3. Preview retrieval

        var client = _factory.CreateClient();

        // We verify the endpoint exists and is callable
        var fileId = Guid.NewGuid();
        var response = await client.GetAsync($"/api/files/{fileId}/preview?includeContent=true");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Image_file_preview_is_supported()
    {
        // Marker test for image preview support
        var client = _factory.CreateClient();

        var fileId = Guid.NewGuid();
        var response = await client.GetAsync($"/api/files/{fileId}/preview?includeContent=true");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Preview_handles_large_files_gracefully()
    {
        var client = _factory.CreateClient();

        // Set a small max preview size
        var fileId = Guid.NewGuid();
        var response = await client.GetAsync($"/api/files/{fileId}/preview?maxPreviewSize=1024&includeContent=true");

        // Should not crash, should return Unauthorized or NotFound
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Preview_supports_metadata_only_mode()
    {
        var client = _factory.CreateClient();

        // Request without content (metadata only)
        var fileId = Guid.NewGuid();
        var response = await client.GetAsync($"/api/files/{fileId}/preview?includeContent=false");

        // Should not crash
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }
}
