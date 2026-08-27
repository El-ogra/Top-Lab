using TopLab.Application.Common.Results;
using Xunit;

namespace TopLab.Application.Tests.Common;

public class ResultsTests
{
    [Fact]
    public void Success_HasNoErrors()
    {
        var r = Result.Success();
        Assert.True(r.IsSuccess);
        Assert.Empty(r.Errors);
    }

    [Fact]
    public void Failure_CarriesSingleError()
    {
        var r = Result.Failure(Error.NotFound("غير موجود"));
        Assert.False(r.IsSuccess);
        Assert.Equal(ErrorType.NotFound, r.Error!.Type);
    }

    [Fact]
    public void Failure_CollectsEveryViolatedRule()
    {
        var errors = new[] { Error.Validation("a"), Error.Validation("b") };
        var r = Result.Failure(errors);
        Assert.Equal(2, r.Errors.Count);
    }

    [Fact]
    public void ResultOfT_Success_CarriesValue()
    {
        var r = Result<string>.Success("x");
        Assert.True(r.IsSuccess);
        Assert.Equal("x", r.Value);
    }

    [Fact]
    public void ResultOfT_Failure_HasNullValue()
    {
        var r = Result<string>.Failure(Error.Unexpected("boom"));
        Assert.False(r.IsSuccess);
        Assert.Null(r.Value);
    }
}
