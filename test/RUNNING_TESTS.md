# Running Tests Guide

## Running All Tests

### From the project root directory:

```bash
# Run all tests in all test projects
dotnet test test

# Or run from the solution file
dotnet test MiniDrive.sln
```

### Run all tests with detailed output:
```bash
dotnet test test --verbosity normal
```

### Run all tests with code coverage:
```bash
dotnet test test --collect:"XPlat Code Coverage"
```

## Running Individual Test Projects

### Run tests for a specific module:
```bash
# Storage tests
dotnet test test/MiniDrive.Storage.Tests

# Files tests
dotnet test test/MiniDrive.Files.Tests

# Folders tests
dotnet test test/MiniDrive.Folders.Tests

# Identity tests
dotnet test test/MiniDrive.Identity.Tests

# Audit tests
dotnet test test/MiniDrive.Audit.Tests

# Quota tests
dotnet test test/MiniDrive.Quota.Tests

# Common tests
dotnet test test/MiniDrive.Common.Tests
```

## Running Individual Test Classes

### Run a specific test class:
```bash
# Example: Run only FileServiceTests
dotnet test test/MiniDrive.Files.Tests --filter "FullyQualifiedName~FileServiceTests"
```

## Running Individual Test Methods

### Run a specific test method:
```bash
# Example: Run only the UploadFileAsync_ValidFile_ReturnsSuccess test
dotnet test test/MiniDrive.Files.Tests --filter "FullyQualifiedName~UploadFileAsync_ValidFile_ReturnsSuccess"
```

### Run tests matching a pattern:
```bash
# Run all tests with "Upload" in the name
dotnet test test --filter "FullyQualifiedName~Upload"
```

## Using Visual Studio

1. **Test Explorer** (View → Test Explorer or Ctrl+E, T)
   - Right-click on a test project → Run Tests
   - Right-click on a test class → Run Tests
   - Right-click on a test method → Run Tests
   - Click the "Run All" button to run all tests

2. **CodeLens** (if enabled)
   - Click "Run" or "Debug" above test methods in the editor

3. **Right-click menu**
   - Right-click on a test file → Run Tests

## Using Visual Studio Code

1. Install the **.NET Core Test Explorer** extension
2. Open the Test Explorer panel
3. Click the play button next to tests you want to run

## Useful Test Commands

### Run tests in parallel (faster):
```bash
dotnet test test --parallel
```

### Run tests without building:
```bash
dotnet test test --no-build
```

### Run tests and show results as they complete:
```bash
dotnet test test --logger "console;verbosity=detailed"
```

### Run tests and stop on first failure:
```bash
dotnet test test --stop-on-first-failure
```

### Run tests with specific framework:
```bash
dotnet test test --framework net10.0
```

## Examples

### Run all Storage tests:
```bash
dotnet test test/MiniDrive.Storage.Tests
```

### Run only FileService tests:
```bash
dotnet test test/MiniDrive.Files.Tests --filter "FullyQualifiedName~FileServiceTests"
```

### Run a specific test method:
```bash
dotnet test test/MiniDrive.Files.Tests --filter "FullyQualifiedName~FileServiceTests.UploadFileAsync_ValidFile_ReturnsSuccess"
```

### Run all tests except a specific project:
```bash
dotnet test test --filter "FullyQualifiedName!~MiniDrive.Sharing.Tests"
```

## Troubleshooting

If tests fail to run:
1. Make sure you're in the project root directory
2. Restore packages: `dotnet restore`
3. Build the solution: `dotnet build`
4. Check that all test projects reference the correct source projects

