# MiniDrive Integration Tests

This document describes the integration tests for all MiniDrive modules.

## Overview

Integration tests verify that multiple components work together correctly, testing the full flow from API endpoints through services and repositories to the database. Unlike unit tests which mock dependencies, integration tests use real implementations with test databases.

## Test Projects

### MiniDrive.Api.IntegrationTests
Full API integration tests using `WebApplicationFactory` to test HTTP endpoints:
- Authentication flows (register, login, logout)
- File operations (upload, download, list, update, delete)
- Folder operations (create, list, update, delete)
- Authorization and security

### MiniDrive.Identity.IntegrationTests
Integration tests for the Identity module:
- User registration and authentication
- Session management
- Password hashing and verification
- User repository operations

### MiniDrive.Files.IntegrationTests
Integration tests for file management:
- File upload and storage
- File download
- File metadata operations
- Quota integration
- Audit logging integration

### MiniDrive.Folders.IntegrationTests
Integration tests for folder management:
- Folder creation and hierarchy
- Folder listing and search
- Folder path resolution
- Folder updates and deletion

### MiniDrive.Audit.IntegrationTests
Integration tests for audit logging:
- Audit log creation
- Logging user actions
- Querying audit logs by user, entity, or action
- Success and failure logging

### MiniDrive.Quota.IntegrationTests
Integration tests for quota management:
- Quota creation and retrieval
- Storage limit checks
- Used bytes tracking (increase/decrease)
- Quota limit updates

### MiniDrive.Storage.IntegrationTests
Integration tests for file storage:
- File save and read operations
- File existence checks
- File deletion
- File size retrieval
- Storage limits

### MiniDrive.Sharing.IntegrationTests
Placeholder tests for the Sharing module (module is currently incomplete).

## Running Integration Tests

### Run All Integration Tests
```bash
dotnet test test --filter "FullyQualifiedName~IntegrationTests"
```

### Run Specific Integration Test Project
```bash
# API integration tests
dotnet test test/MiniDrive.Api.IntegrationTests

# Identity integration tests
dotnet test test/MiniDrive.Identity.IntegrationTests

# Files integration tests
dotnet test test/MiniDrive.Files.IntegrationTests

# Folders integration tests
dotnet test test/MiniDrive.Folders.IntegrationTests

# Audit integration tests
dotnet test test/MiniDrive.Audit.IntegrationTests

# Quota integration tests
dotnet test test/MiniDrive.Quota.IntegrationTests

# Storage integration tests
dotnet test test/MiniDrive.Storage.IntegrationTests

# Sharing integration tests
dotnet test test/MiniDrive.Sharing.IntegrationTests
```

### Run Specific Test Class
```bash
dotnet test test/MiniDrive.Api.IntegrationTests --filter "FullyQualifiedName~ApiIntegrationTests"
```

### Run Specific Test Method
```bash
dotnet test test/MiniDrive.Identity.IntegrationTests --filter "FullyQualifiedName~RegisterAsync_ValidUser_CreatesUserAndSession"
```

## Test Infrastructure

### Database Setup
- Integration tests use **SQLite in-memory databases** for fast execution
- Each test creates a fresh database instance
- Databases are cleaned up after each test

### API Testing
- Uses `WebApplicationFactory` to create a test server
- Tests make real HTTP requests to the API
- In-memory databases replace production databases
- Temporary storage directories are used for file operations

### Test Isolation
- Each test class creates its own database instance
- Tests are independent and can run in parallel
- Temporary files and directories are cleaned up after tests

## Test Coverage

Integration tests cover:
- ✅ End-to-end API workflows
- ✅ Service and repository integration
- ✅ Database operations
- ✅ File storage operations
- ✅ Cross-module interactions (e.g., Files + Quota + Audit)
- ✅ Error handling and edge cases
- ✅ Authorization and security

## Differences from Unit Tests

| Aspect | Unit Tests | Integration Tests |
|--------|-----------|-------------------|
| Dependencies | Mocked | Real implementations |
| Database | In-memory (EF Core) | SQLite in-memory |
| API Testing | Direct service calls | HTTP requests via WebApplicationFactory |
| Execution Speed | Fast | Slower (but still fast with in-memory DB) |
| Coverage | Individual components | Full workflows |

## Best Practices

1. **Test Real Workflows**: Integration tests should test complete user workflows, not just individual methods
2. **Clean Up**: Always clean up test data and temporary files
3. **Isolation**: Each test should be independent and not rely on other tests
4. **Use Appropriate Assertions**: Use FluentAssertions for readable test assertions
5. **Test Both Success and Failure**: Include tests for error cases and edge conditions

## Troubleshooting

### Tests Fail with Database Errors
- Ensure SQLite is available (included with .NET)
- Check that database contexts are properly disposed

### Tests Fail with File System Errors
- Ensure test processes have write permissions
- Check that temporary directories are cleaned up

### API Tests Fail with Connection Errors
- Verify `WebApplicationFactory` is properly configured
- Check that all required services are registered

## Next Steps

When adding new integration tests:
1. Follow the existing test structure
2. Use SQLite in-memory databases for database tests
3. Use temporary directories for file storage tests
4. Clean up resources in `Dispose()` methods
5. Add tests for both success and failure scenarios

