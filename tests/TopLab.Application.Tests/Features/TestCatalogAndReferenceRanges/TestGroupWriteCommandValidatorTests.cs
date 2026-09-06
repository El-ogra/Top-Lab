using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTestGroup;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeactivateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeactivateTestGroup;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.ReactivateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.ReactivateTestGroup;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateTestGroup;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

public class TestGroupWriteCommandValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateGroupValidator_NameWhitespaceOrNull_Invalid(string? name)
    {
        var validator = new CreateTestGroupCommandValidator();

        var result = validator.Validate(new CreateTestGroupCommand(name!));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateGroupValidator_NameOver150_Invalid()
    {
        var validator = new CreateTestGroupCommandValidator();

        var result = validator.Validate(new CreateTestGroupCommand(new string('ن', 151)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateGroupValidator_ValidName_Passes()
    {
        var validator = new CreateTestGroupCommandValidator();

        var result = validator.Validate(new CreateTestGroupCommand("الهيماتولوجيا"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateGroupValidator_NameWhitespaceOrNull_Invalid(string? name)
    {
        var validator = new UpdateTestGroupCommandValidator();

        var result = validator.Validate(new UpdateTestGroupCommand(1, name!));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void UpdateGroupValidator_NameOver150_Invalid()
    {
        var validator = new UpdateTestGroupCommandValidator();

        var result = validator.Validate(new UpdateTestGroupCommand(1, new string('ن', 151)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateGroupValidator_Valid_Passes()
    {
        var validator = new UpdateTestGroupCommandValidator();

        var result = validator.Validate(new UpdateTestGroupCommand(1, "الكيمياء"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeactivateGroupValidator_InvalidId_Invalid(int id)
    {
        var validator = new DeactivateTestGroupCommandValidator();

        var result = validator.Validate(new DeactivateTestGroupCommand(id));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact]
    public void DeactivateGroupValidator_ValidId_Passes()
    {
        var validator = new DeactivateTestGroupCommandValidator();

        var result = validator.Validate(new DeactivateTestGroupCommand(1));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReactivateGroupValidator_InvalidId_Invalid(int id)
    {
        var validator = new ReactivateTestGroupCommandValidator();

        var result = validator.Validate(new ReactivateTestGroupCommand(id));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact]
    public void ReactivateGroupValidator_ValidId_Passes()
    {
        var validator = new ReactivateTestGroupCommandValidator();

        var result = validator.Validate(new ReactivateTestGroupCommand(1));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeactivateTestValidator_InvalidId_Invalid(int id)
    {
        var validator = new DeactivateTestCommandValidator();

        var result = validator.Validate(new DeactivateTestCommand(id));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact]
    public void DeactivateTestValidator_ValidId_Passes()
    {
        var validator = new DeactivateTestCommandValidator();

        var result = validator.Validate(new DeactivateTestCommand(1));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReactivateTestValidator_InvalidId_Invalid(int id)
    {
        var validator = new ReactivateTestCommandValidator();

        var result = validator.Validate(new ReactivateTestCommand(id));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact]
    public void ReactivateTestValidator_ValidId_Passes()
    {
        var validator = new ReactivateTestCommandValidator();

        var result = validator.Validate(new ReactivateTestCommand(1));

        Assert.True(result.IsValid);
    }
}