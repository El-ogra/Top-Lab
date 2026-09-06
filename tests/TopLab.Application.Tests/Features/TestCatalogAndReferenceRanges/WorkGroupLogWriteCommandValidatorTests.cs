using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateWorkGroupLog;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.RenameWorkGroupLog;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.SaveWorkGroupLogItems;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

public class WorkGroupLogWriteCommandValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateLogValidator_NameWhitespaceOrNull_Invalid(string? name)
    {
        var validator = new CreateWorkGroupLogCommandValidator();

        var result = validator.Validate(new CreateWorkGroupLogCommand(name!));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateLogValidator_NameOver150_Invalid()
    {
        var validator = new CreateWorkGroupLogCommandValidator();

        var result = validator.Validate(new CreateWorkGroupLogCommand(new string('ن', 151)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateLogValidator_ValidName_Passes()
    {
        var validator = new CreateWorkGroupLogCommandValidator();

        var result = validator.Validate(new CreateWorkGroupLogCommand("ورشة المناعة"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RenameLogValidator_NameWhitespaceOrNull_Invalid(string? name)
    {
        var validator = new RenameWorkGroupLogCommandValidator();

        var result = validator.Validate(new RenameWorkGroupLogCommand(1, name!));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void RenameLogValidator_Valid_Passes()
    {
        var validator = new RenameWorkGroupLogCommandValidator();

        var result = validator.Validate(new RenameWorkGroupLogCommand(1, "ورشة الكيمياء"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void SaveItemsValidator_EmptyTestIds_Invalid()
    {
        var validator = new SaveWorkGroupLogItemsCommandValidator();

        var result = validator.Validate(new SaveWorkGroupLogItemsCommand(1, Array.Empty<int>()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TestIds");
    }

    [Fact]
    public void SaveItemsValidator_DuplicateTestIds_Invalid()
    {
        var validator = new SaveWorkGroupLogItemsCommandValidator();

        var result = validator.Validate(new SaveWorkGroupLogItemsCommand(1, new[] { 1, 1, 2 }));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TestIds");
    }

    [Fact]
    public void SaveItemsValidator_DistinctTestIds_Passes()
    {
        var validator = new SaveWorkGroupLogItemsCommandValidator();

        var result = validator.Validate(new SaveWorkGroupLogItemsCommand(1, new[] { 1, 2, 3 }));

        Assert.True(result.IsValid);
    }
}