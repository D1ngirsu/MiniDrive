using Xunit;
using MiniDrive.Common;

namespace MiniDrive.UnitTests;

public class ResultTests
{
    [Fact]
    public void Success_creates_succeeded_result()
    {
        var r = Result.Success();
        Assert.True(r.Succeeded);
        Assert.Null(r.Error);
    }

    [Fact]
    public void Failure_trims_and_defaults_error()
    {
        var r = Result.Failure("  something  ");
        Assert.False(r.Succeeded);
        Assert.Equal("something", r.Error);
    }

    [Fact]
    public void Failure_with_null_error_defaults_to_unknown()
    {
        var r = Result.Failure(null);
        Assert.False(r.Succeeded);
        Assert.Equal("Unknown error.", r.Error);
    }

    [Fact]
    public void Failure_with_empty_string_defaults_to_unknown()
    {
        var r = Result.Failure("   ");
        Assert.False(r.Succeeded);
        Assert.Equal("Unknown error.", r.Error);
    }

    [Fact]
    public void Generic_Success_carries_value()
    {
        var r = Result<int>.Success(5);
        Assert.True(r.Succeeded);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Generic_Success_with_null_value()
    {
        var r = Result<string>.Success(null!);
        Assert.True(r.Succeeded);
        Assert.Null(r.Value);
    }

    [Fact]
    public void Generic_Failure_has_null_value()
    {
        var r = Result<string>.Failure("error");
        Assert.False(r.Succeeded);
        Assert.Null(r.Value);
        Assert.Equal("error", r.Error);
    }

    [Fact]
    public void Generic_Failure_with_empty_error_defaults_to_unknown()
    {
        var r = Result<int>.Failure("");
        Assert.False(r.Succeeded);
        Assert.Equal("Unknown error.", r.Error);
    }
}
