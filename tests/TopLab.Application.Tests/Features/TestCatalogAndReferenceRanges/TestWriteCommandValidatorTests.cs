using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateTest;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Tests.Features.TestCatalogAndReferenceRanges;

public class TestWriteCommandValidatorTests
{
    private static CreateTestCommand ValidCreate() => new(
        "تحليل عام", "تقرير تحليل عام", "إيصال تحليل عام", "CBC",
        30, 150m, ResultKind.Simple, false, null, null, false, null, null);

    private static UpdateTestCommand ValidUpdate() => new(
        1, "تحليل عام", "تقرير تحليل عام", "إيصال تحليل عام", "CBC",
        30, 150m, null, null, false, null, null);

    // ---------- CreateTestCommandValidator ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateValidator_TestCodeWhitespaceOrNull_Invalid(string? testCode)
    {
        var validator = new CreateTestCommandValidator();

        var result = validator.Validate(ValidCreate() with { TestCode = testCode! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TestCode");
    }

    [Fact]
    public void CreateValidator_TestCodeOver50_Invalid()
    {
        var validator = new CreateTestCommandValidator();

        var result = validator.Validate(ValidCreate() with { TestCode = new string('X', 51) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TestCode");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateValidator_NameWhitespaceOrNull_Invalid(string? name)
    {
        var validator = new CreateTestCommandValidator();

        var result = validator.Validate(ValidCreate() with { Name = name! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateValidator_NameOver150_Invalid()
    {
        var validator = new CreateTestCommandValidator();

        var result = validator.Validate(ValidCreate() with { Name = new string('ن', 151) });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateValidator_ReportNameMissing_Invalid()
    {
        var validator = new CreateTestCommandValidator();

        var result = validator.Validate(ValidCreate() with { ReportName = "" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ReportName");
    }

    [Fact]
    public void CreateValidator_ReceiptNameMissing_Invalid()
    {
        var validator = new CreateTestCommandValidator();

        var result = validator.Validate(ValidCreate() with { ReceiptName = "" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ReceiptName");
    }

    [Fact]
    public void CreateValidator_BarcodeOver50_Invalid()
    {
        var validator = new CreateTestCommandValidator();

        var result = validator.Validate(ValidCreate() with { Barcode = new string('B', 51) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Barcode");
    }

    [Fact]
    public void CreateValidator_ZeroDuration_Invalid()
    {
        var validator = new CreateTestCommandValidator();

        var result = validator.Validate(ValidCreate() with { CompletionDurationMinutes = 0 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CompletionDurationMinutes"
            && e.ErrorMessage == "مدة الإنجاز يجب أن تكون أكبر من صفر.");
    }

    [Fact]
    public void CreateValidator_NegativePatientPrice_Invalid()
    {
        var validator = new CreateTestCommandValidator();

        var result = validator.Validate(ValidCreate() with { PatientPrice = -1m });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PatientPrice");
    }

    [Fact]
    public void CreateValidator_NegativeLabToLabPrice_Invalid()
    {
        var validator = new CreateTestCommandValidator();

        var result = validator.Validate(ValidCreate() with { LabToLabPrice = -1m });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LabToLabPrice");
    }

    [Fact]
    public void CreateValidator_SentOutWithoutCost_Invalid()
    {
        var validator = new CreateTestCommandValidator();

        var result = validator.Validate(ValidCreate() with { IsSentOut = true, SentOutCostPrice = null });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "سعر الإرسال مطلوب عندما يكون التحليل صادرًا.");
    }

    [Fact]
    public void CreateValidator_SentOutWithCost_Valid()
    {
        var validator = new CreateTestCommandValidator();

        var result = validator.Validate(ValidCreate() with { IsSentOut = true, SentOutCostPrice = 500m });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateValidator_InvalidResultKind_Invalid()
    {
        var validator = new CreateTestCommandValidator();

        var result = validator.Validate(ValidCreate() with { ResultKind = (ResultKind)99 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ResultKind");
    }

    [Fact]
    public void CreateValidator_ValidCommand_Passes()
    {
        var validator = new CreateTestCommandValidator();

        var result = validator.Validate(ValidCreate());

        Assert.True(result.IsValid);
    }

    // ---------- UpdateTestCommandValidator ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateValidator_TestCodeWhitespaceOrNull_Invalid(string? testCode)
    {
        var validator = new UpdateTestCommandValidator();

        var result = validator.Validate(ValidUpdate() with { TestCode = testCode! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TestCode");
    }

    [Fact]
    public void UpdateValidator_TestCodeOver50_Invalid()
    {
        var validator = new UpdateTestCommandValidator();

        var result = validator.Validate(ValidUpdate() with { TestCode = new string('X', 51) });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateValidator_ZeroDuration_Invalid()
    {
        var validator = new UpdateTestCommandValidator();

        var result = validator.Validate(ValidUpdate() with { CompletionDurationMinutes = 0 });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateValidator_NegativePatientPrice_Invalid()
    {
        var validator = new UpdateTestCommandValidator();

        var result = validator.Validate(ValidUpdate() with { PatientPrice = -1m });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateValidator_SentOutWithoutCost_Invalid()
    {
        var validator = new UpdateTestCommandValidator();

        var result = validator.Validate(ValidUpdate() with { IsSentOut = true, SentOutCostPrice = null });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateValidator_ValidCommand_Passes()
    {
        var validator = new UpdateTestCommandValidator();

        var result = validator.Validate(ValidUpdate());

        Assert.True(result.IsValid);
    }
}