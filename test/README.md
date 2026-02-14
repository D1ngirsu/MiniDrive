# Tests

Run all tests from the repository root:

```bash
dotnet test
```

Run only unit tests:

```bash
dotnet test test/UnitTests
```

Run only integration tests:

```bash
dotnet test test/IntegrationTests
```

Notes:
- Integration tests use `WebApplicationFactory<Program>`; projects include a small `Program.Partial.cs` file so the `Program` type is available for the test host.
- Integration tests run the API in the `Testing` environment (set in the test host).
