# MiniDrive Unit Tests

This folder contains unit tests for all modules in the MiniDrive project.

## Test Projects

- **MiniDrive.Storage.Tests** - Tests for file storage operations
- **MiniDrive.Common.Tests** - Tests for common utilities (Result, caching)
- **MiniDrive.Files.Tests** - Tests for file management services and repositories
- **MiniDrive.Folders.Tests** - Tests for folder management services and repositories
- **MiniDrive.Identity.Tests** - Tests for authentication services and repositories
- **MiniDrive.Audit.Tests** - Tests for audit logging services and repositories
- **MiniDrive.Quota.Tests** - Tests for quota management services and repositories

## Running Tests

### Using .NET CLI
```bash
dotnet test
```

### Using Visual Studio
1. Open Test Explorer (Test > Test Explorer)
2. Run All Tests

### Running specific test project
```bash
dotnet test test/MiniDrive.Storage.Tests
```

## Test Structure

Each test project follows a similar structure:
- **Service Tests** - Test business logic with mocked dependencies
- **Repository Tests** - Test data access using in-memory databases

## Testing Frameworks

- **xUnit** - Test framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for repository tests

## Test Coverage

Tests cover:
- ✅ Happy path scenarios
- ✅ Error handling and validation
- ✅ Edge cases
- ✅ Security checks (path traversal, authorization)
- ✅ Business logic validation

