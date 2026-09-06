using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateReferenceRange;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeleteReferenceRange;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateReferenceRange;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

public class ReferenceRangeWriteCommandValidatorTests
{
    private static CreateReferenceRangeCommand ValidCreate() => new(
        1, null, AgeUnit.Year, 0, 120, 0m, 100m, null, null);

    private static UpdateReferenceRangeCommand ValidUpdate() => new(
        1, null, AgeUnit.Year, 0, 120, 0m, 100m, null, null);

    // ---------- CreateReferenceRangeCommandValidator ----------

    [Fact]
    public void CreateValidator_NegativeAgeMin_Invalid()
    {
        var validator = new CreateReferenceRangeCommandValidator();

        var result = validator.Validate(ValidCreate() with { AgeMin = -1 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AgeMin");
    }

    [Fact]
    public void CreateValidator_AgeMaxBelowAgeMin_Invalid()
    {
        var validator = new CreateReferenceRangeCommandValidator();

        var result = validator.Validate(ValidCreate() with { AgeMin = 50, AgeMax = 40 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AgeMax");
    }

    [Fact]
    public void CreateValidator_MinValueAboveMaxValue_Invalid()
    {
        var validator = new CreateReferenceRangeCommandValidator();

        var result = validator.Validate(ValidCreate() with { MinValue = 10m, MaxValue = 5m });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "MinValue");
    }

    [Fact]
    public void CreateValidator_InvalidAgeUnit_Invalid()
    {
        var validator = new CreateReferenceRangeCommandValidator();

        var result = validator.Validate(ValidCreate() with { AgeUnit = (AgeUnit)99 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AgeUnit");
    }

    [Fact]
    public void CreateValidator_InvalidSex_Invalid()
    {
        var validator = new CreateReferenceRangeCommandValidator();

        var result = validator.Validate(ValidCreate() with { Sex = (Sex)99 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Sex");
    }

    [Fact]
    public void CreateValidator_LowCommentOver500_Invalid()
    {
        var validator = new CreateReferenceRangeCommandValidator();

        var result = validator.Validate(ValidCreate() with { LowComment = new string('ن', 501) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LowComment");
    }

    [Fact]
    public void CreateValidator_HighCommentOver500_Invalid()
    {
        var validator = new CreateReferenceRangeCommandValidator();

        var result = validator.Validate(ValidCreate() with { HighComment = new string('ن', 501) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "HighComment");
    }

    [Fact]
    public void CreateValidator_NullableSexPasses()
    {
        var validator = new CreateReferenceRangeCommandValidator();

        var result = validator.Validate(ValidCreate());

        Assert.True(result.IsValid);
    }

    // ---------- UpdateReferenceRangeCommandValidator ----------

    [Fact]
    public void UpdateValidator_NegativeAgeMin_Invalid()
    {
        var validator = new UpdateReferenceRangeCommandValidator();

        var result = validator.Validate(ValidUpdate() with { AgeMin = -1 });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateValidator_AgeMaxBelowAgeMin_Invalid()
    {
        var validator = new UpdateReferenceRangeCommandValidator();

        var result = validator.Validate(ValidUpdate() with { AgeMin = 50, AgeMax = 40 });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateValidator_MinValueAboveMaxValue_Invalid()
    {
        var validator = new UpdateReferenceRangeCommandValidator();

        var result = validator.Validate(ValidUpdate() with { MinValue = 10m, MaxValue = 5m });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateValidator_InvalidAgeUnit_Invalid()
    {
        var validator = new UpdateReferenceRangeCommandValidator();

        var result = validator.Validate(ValidUpdate() with { AgeUnit = (AgeUnit)99 });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateValidator_CommentsOver500_Invalid()
    {
        var validator = new UpdateReferenceRangeCommandValidator();

        var result = validator.Validate(ValidUpdate() with { LowComment = new string('ن', 501), HighComment = new string('ن', 501) });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateValidator_Valid_Passes()
    {
        var validator = new UpdateReferenceRangeCommandValidator();

        var result = validator.Validate(ValidUpdate());

        Assert.True(result.IsValid);
    }

    // ---------- DeleteReferenceRangeCommandValidator ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeleteValidator_InvalidId_Invalid(int id)
    {
        var validator = new DeleteReferenceRangeCommandValidator();

        var result = validator.Validate(new DeleteReferenceRangeCommand(id));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact]
    public void DeleteValidator_ValidId_Passes()
    {
        var validator = new DeleteReferenceRangeCommandValidator();

        var result = validator.Validate(new DeleteReferenceRangeCommand(1));

        Assert.True(result.IsValid);
    }
}