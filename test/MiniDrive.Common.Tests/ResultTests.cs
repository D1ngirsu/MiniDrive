using FluentAssertions;
using MiniDrive.Common;
using Xunit;

namespace MiniDrive.Common.Tests;

public class ResultTests
{
    [Fact]
    public void Success_NonGeneric_ReturnsSucceededResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_NonGeneric_ReturnsFailedResult()
    {
        // Arrange
        var errorMessage = "Something went wrong";

        // Act
        var result = Result.Failure(errorMessage);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(errorMessage);
    }

    [Fact]
    public void Failure_NonGeneric_WithWhitespace_ReturnsDefaultError()
    {
        // Act
        var result = Result.Failure("   ");

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Unknown error.");
    }

    [Fact]
    public void Success_Generic_ReturnsSucceededResultWithValue()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.Should().Be(value);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_Generic_ReturnsFailedResult()
    {
        // Arrange
        var errorMessage = "Something went wrong";

        // Act
        var result = Result<string>.Failure(errorMessage);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be(errorMessage);
    }

    [Fact]
    public void Failure_Generic_WithWhitespace_ReturnsDefaultError()
    {
        // Act
        var result = Result<string>.Failure("   ");

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be("Unknown error.");
    }

    [Fact]
    public void Success_Generic_WithNullValue_ReturnsSucceededResult()
    {
        // Act
        var result = Result<string?>.Success(null);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void Failure_Generic_TrimsErrorMessage()
    {
        // Arrange
        var errorMessage = "  Something went wrong  ";

        // Act
        var result = Result<string>.Failure(errorMessage);

        // Assert
        result.Error.Should().Be("Something went wrong");
    }
}

