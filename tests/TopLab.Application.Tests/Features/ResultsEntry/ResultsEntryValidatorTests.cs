using TopLab.Application.Features.ResultsEntry.Commands.EnterResult;
using TopLab.Application.Features.ResultsEntry.Commands.ExportPatientReportPdf;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

public class ResultsEntryValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnterResultValidator_Rejects_NullEmptyWhitespace_WithExactMessage(string? value)
    {
        var validator = new EnterResultCommandValidator();
        var result = validator.Validate(new EnterResultCommand(1, value));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "الرجاء إدخال قيمة النتيجة قبل الحفظ");
    }

    [Theory]
    [InlineData("5")]
    [InlineData("Positive")]
    public void EnterResultValidator_Accepts_NonWhitespace(string value)
    {
        var validator = new EnterResultCommandValidator();
        var result = validator.Validate(new EnterResultCommand(1, value));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ExportValidator_Rejects_RelativePath()
    {
        var validator = new ExportPatientReportPdfCommandValidator();
        var result = validator.Validate(new ExportPatientReportPdfCommand(1, "relative.pdf"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ExportValidator_Rejects_NonPdf()
    {
        var absolute = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "report.txt");
        var validator = new ExportPatientReportPdfCommandValidator();
        var result = validator.Validate(new ExportPatientReportPdfCommand(1, absolute));

        Assert.False(result.IsValid);
    }
}
