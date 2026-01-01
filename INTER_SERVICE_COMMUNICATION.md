# Inter-Service Communication Guide

This document explains how microservices communicate in the MiniDrive architecture.

## Communication Patterns Overview

The architecture uses **multiple communication patterns** depending on the use case:

1. **Synchronous HTTP** - For request/response operations
2. **API Gateway Pattern** - For external client routing
3. **Adapter Pattern** - For seamless integration with existing code
4. **Fire-and-Forget** - For non-critical operations (audit logging)

---

## 1. Synchronous HTTP Communication

### How It Works

Services communicate via **HTTP REST APIs** using typed HTTP clients from the `MiniDrive.Clients` library.

### Example: Files Service → Identity Service

When a user uploads a file, the Files service needs to validate the authentication token:

```csharp
// In Files.Api/Controllers/FileController.cs
var userId = await GetUserIdAsync(authorization);
// ↓
// Calls IIdentityClient.ValidateSessionAsync()
// ↓
// Makes HTTP GET request to Identity service
// GET http://localhost:5001/api/Auth/me
// Headers: Authorization: Bearer <token>
```

**Flow:**
```
Client Request → Files.Api
    ↓
FileController.GetUserIdAsync()
    ↓
IIdentityClient.ValidateSessionAsync()
    ↓
HTTP GET http://localhost:5001/api/Auth/me
    ↓
Identity.Api responds with UserInfo
    ↓
FileController continues processing
```

### HTTP Client Configuration

HTTP clients are registered in each service's `Program.cs`:

```csharp
// In Files.Api/Program.cs
var identityServiceUrl = builder.Configuration["Services:Identity"] ?? "http://localhost:5001";
builder.Services.AddHttpClient<IIdentityClient, IdentityClient>(client =>
{
    client.BaseAddress = new Uri(identityServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

### Client Library Structure

All HTTP clients are in `MiniDrive.Clients`:

```
MiniDrive.Clients/
├── Identity/
│   ├── IIdentityClient.cs       (interface)
│   └── IdentityClient.cs        (HTTP implementation)
├── Quota/
│   ├── IQuotaClient.cs
│   └── QuotaClient.cs
└── Audit/
    ├── IAuditClient.cs
    └── AuditClient.cs
```

**Benefits:**
- ✅ Type-safe communication
- ✅ Centralized HTTP logic
- ✅ Easy to mock for testing
- ✅ Configuration via dependency injection

---

## 2. Adapter Pattern for Seamless Integration

### Problem

The `FileService` expects `IQuotaService` and `IAuditService` interfaces, but we want to call microservices via HTTP instead of direct dependencies.

### Solution: Adapter Classes

Adapters implement the domain interfaces but delegate to HTTP clients:

```csharp
// In Files.Api/Adapters/QuotaServiceAdapter.cs
public class QuotaServiceAdapter : IQuotaService
{
    private readonly IQuotaClient _quotaClient;  // HTTP client
    
    public async Task<bool> CanUploadAsync(Guid userId, long fileSize)
    {
        // Calls Quota microservice via HTTP
        return await _quotaClient.CanUploadAsync(userId, fileSize);
    }
    
    // Implements all IQuotaService methods...
}
```

**Registration:**
```csharp
// In Files.Api/Program.cs
// Register adapter to replace the direct service
builder.Services.AddScoped<IQuotaService, QuotaServiceAdapter>();
builder.Services.AddScoped<IAuditService, AuditServiceAdapter>();

// FileService still uses IQuotaService interface - no code changes needed!
builder.Services.AddScoped<FileService>();
```

**Benefits:**
- ✅ Zero changes to domain services (`FileService` stays the same)
- ✅ Clean separation of concerns
- ✅ Easy to switch back to direct dependencies if needed

---

## 3. Communication Flow Examples

### Example 1: File Upload (Complex Flow)

```
1. Client → POST /api/File/upload (via Gateway)
   ↓
2. Gateway → Routes to Files.Api:5002
   ↓
3. Files.Api/FileController.UploadFile()
   ↓
4. FileController calls IIdentityClient.ValidateSessionAsync()
   → HTTP GET Identity.Api:5001/api/Auth/me
   ← Returns UserInfo { Id: xxx, Email: "..." }
   ↓
5. FileController calls FileService.UploadFileAsync()
   ↓
6. FileService calls IQuotaService.CanUploadAsync()
   → Adapter → HTTP GET Quota.Api:5004/api/Quota/{userId}/can-upload
   ← Returns { canUpload: true }
   ↓
7. FileService saves file to storage
   ↓
8. FileService calls IQuotaService.IncreaseAsync()
   → Adapter → HTTP POST Quota.Api:5004/api/Quota/{userId}/increase
   ← Returns { success: true }
   ↓
9. FileService calls IAuditService.LogActionAsync()
   → Adapter → HTTP POST Audit.Api:5005/api/Audit/log (fire-and-forget)
   ↓
10. FileController returns response to client
```

### Example 2: Folder Creation (Simple Flow)

```
1. Client → POST /api/Folder (via Gateway)
   ↓
2. Gateway → Routes to Folders.Api:5003
   ↓
3. Folders.Api/FolderController.CreateFolder()
   ↓
4. FolderController calls IIdentityClient.ValidateSessionAsync()
   → HTTP GET Identity.Api:5001/api/Auth/me
   ↓
5. FolderController calls FolderService.CreateFolderAsync()
   ↓
6. FolderController returns response
```

---

## 4. API Gateway Pattern

### Purpose

The API Gateway provides a **single entry point** for external clients.

### How It Works

**YARP (Yet Another Reverse Proxy)** routes requests based on URL patterns:

```json
{
  "ReverseProxy": {
    "Routes": {
      "files-route": {
        "ClusterId": "files-cluster",
        "Match": { "Path": "/api/File/{**catch-all}" }
      }
    },
    "Clusters": {
      "files-cluster": {
        "Destinations": {
          "destination1": { "Address": "http://localhost:5002/" }
        }
      }
    }
  }
}
```

### Request Flow

```
External Client
    ↓
GET http://localhost:5000/api/File/123
    ↓
Gateway.Api (Port 5000)
    ↓
YARP matches "/api/File/**" → routes to files-cluster
    ↓
Files.Api (Port 5002)
    ↓
Response → Gateway → Client
```

**Benefits:**
- ✅ Single endpoint for clients
- ✅ Load balancing support
- ✅ SSL termination in one place
- ✅ Request/response transformation

---

## 5. Fire-and-Forget Pattern (Audit Logging)

For non-critical operations like audit logging, failures shouldn't break the main flow:

```csharp
// In AuditClient.cs
public async Task LogActionAsync(...)
{
    try
    {
        await _httpClient.PostAsJsonAsync("/api/Audit/log", request);
        // Fire and forget - don't throw on failure
    }
    catch
    {
        // Silently fail - audit logging should not break the main flow
    }
}
```

**Use Cases:**
- ✅ Audit logging
- ✅ Analytics events
- ✅ Notifications (email, SMS)

---

## 6. Configuration and Service Discovery

### Static Configuration (Current)

Service URLs are configured in `appsettings.json`:

```json
{
  "Services": {
    "Identity": "http://localhost:5001",
    "Quota": "http://localhost:5004",
    "Audit": "http://localhost:5005"
  }
}
```

### Future: Service Discovery

For production, consider:
- **Consul** - Service discovery and health checking
- **Kubernetes Services** - Built-in service discovery
- **Ocelot** - More advanced API Gateway with service discovery

---

## 7. Error Handling and Resilience

### Current Implementation

HTTP clients handle errors gracefully:

```csharp
public async Task<UserInfo?> ValidateSessionAsync(string token)
{
    try
    {
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return null;  // Fail gracefully
        }
        // ... deserialize and return
    }
    catch
    {
        return null;  // Network errors return null
    }
}
```

### Recommended Improvements

1. **Retry Policy** (using Polly):
```csharp
builder.Services.AddHttpClient<IQuotaClient, QuotaClient>()
    .AddPolicyHandler(GetRetryPolicy());

Policy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
```

2. **Circuit Breaker Pattern**:
```csharp
.AddPolicyHandler(GetCircuitBreakerPolicy());

Policy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
```

3. **Timeout Configuration**:
```csharp
client.Timeout = TimeSpan.FromSeconds(30);  // Already configured
```

---

## 8. Communication Patterns Summary

| Pattern | Use Case | Example |
|---------|----------|---------|
| **Synchronous HTTP** | Request/response operations | Validate token, check quota |
| **Adapter Pattern** | Maintain existing interfaces | FileService using IQuotaService |
| **API Gateway** | External client routing | All client requests |
| **Fire-and-Forget** | Non-critical operations | Audit logging |
| **Static URLs** | Development/testing | Current implementation |
| **Service Discovery** | Production | Future enhancement |

---

## 9. Testing Inter-Service Communication

### Unit Tests

Mock the HTTP clients:

```csharp
var mockIdentityClient = new Mock<IIdentityClient>();
mockIdentityClient
    .Setup(x => x.ValidateSessionAsync(It.IsAny<string>()))
    .ReturnsAsync(new UserInfo { Id = userId });

// Inject mock into controller/service
```

### Integration Tests

Use `WebApplicationFactory` to test actual HTTP communication:

```csharp
var factory = new WebApplicationFactory<Program>();
var client = factory.CreateClient();

var response = await client.GetAsync("/api/File/123");
```

---

## 10. Best Practices

1. ✅ **Use typed clients** - Don't use `HttpClient` directly
2. ✅ **Configure timeouts** - Prevent hanging requests
3. ✅ **Handle failures gracefully** - Don't let service failures cascade
4. ✅ **Use adapters** - Maintain clean domain boundaries
5. ✅ **Log all service calls** - For debugging and monitoring
6. ✅ **Version your APIs** - For future compatibility
7. ✅ **Use API Gateway** - Single entry point for clients
8. ✅ **Implement health checks** - Monitor service availability

---

## Next Steps

- [ ] Add retry policies with Polly
- [ ] Implement circuit breaker pattern
- [ ] Add distributed tracing (OpenTelemetry)
- [ ] Set up service discovery (Consul/Kubernetes)
- [ ] Add request/response logging middleware
- [ ] Implement API versioning
- [ ] Add health check endpoints

