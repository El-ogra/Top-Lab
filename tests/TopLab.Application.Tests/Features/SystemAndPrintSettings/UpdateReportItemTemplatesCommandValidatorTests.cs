using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportItemTemplates;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class UpdateReportItemTemplatesCommandValidatorTests
{
    private readonly UpdateReportItemTemplatesCommandValidator _validator = new();

    private static ReportItemTemplateDto Dto(string key, string label) =>
        new(key, label, null, true, null, false);

    private static UpdateReportItemTemplatesCommand Valid() =>
        new([Dto(ReportItemKeys.TestName, "التحليل"), Dto(ReportItemKeys.ResultValue, "النتيجة")]);

    [Fact]
    public void Valid_Passes()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Fact]
    public void NullItems_Fails()
    {
        var result = _validator.Validate(new UpdateReportItemTemplatesCommand(null!));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UnknownKey_Fails()
    {
        var result = _validator.Validate(new UpdateReportItemTemplatesCommand([Dto("NotAKey", "x")]));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void DuplicateKeys_Fail()
    {
        var result = _validator.Validate(new UpdateReportItemTemplatesCommand(
            [Dto(ReportItemKeys.TestName, "التحليل"), Dto(ReportItemKeys.TestName, "أخرى")]));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void BlankLabel_Fails()
    {
        var result = _validator.Validate(new UpdateReportItemTemplatesCommand([Dto(ReportItemKeys.TestName, "  ")]));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void OverLongLabel_Fails()
    {
        var label = new string('ع', ReportItemTemplate.MaxLabelLength + 1);
        var result = _validator.Validate(new UpdateReportItemTemplatesCommand([Dto(ReportItemKeys.TestName, label)]));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void OverLongReferenceExpression_Fails()
    {
        var value = new string('x', ReportItemTemplate.MaxReferenceValueExpressionLength + 1);
        var result = _validator.Validate(new UpdateReportItemTemplatesCommand(
            [new ReportItemTemplateDto(ReportItemKeys.TestName, "التحليل", value, true, null, false)]));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void OverLongListMembership_Fails()
    {
        var value = new string('x', ReportItemTemplate.MaxListMembershipLength + 1);
        var result = _validator.Validate(new UpdateReportItemTemplatesCommand(
            [new ReportItemTemplateDto(ReportItemKeys.TestName, "التحليل", null, true, value, false)]));
        Assert.False(result.IsValid);
    }
}
