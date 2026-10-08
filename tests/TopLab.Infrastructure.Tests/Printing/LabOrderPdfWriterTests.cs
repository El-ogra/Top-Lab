using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

public class LabOrderPdfWriterTests
{
    private static LabOrderDto Order(params LabOrderLineDto[] lines)
    {
        return new LabOrderDto(
            7,
            "أحمد محمد علي",
            "100",
            "100",
            new DateTime(2026, 10, 1, 9, 30, 0, DateTimeKind.Utc),
            lines.ToList());
    }

    private static LabPrintTextDto LabText()
    {
        return new LabPrintTextDto("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12);
    }

    [Fact]
    public void BuildTextLines_MapsPatientBlockTestLinesAndPayload()
    {
        var lines = LabOrderPdfWriter.BuildTextLines(
            Order(new LabOrderLineDto("CBC", "صورة دم كاملة"), new LabOrderLineDto("GLU", "Glucose")),
            LabText());

        Assert.Contains("مختبر الشفاء", lines.Header);
        Assert.Contains("أحمد محمد علي", lines.Patient[0]);
        Assert.Contains("100", lines.Patient[1]);
        Assert.Contains("2026-10-01 09:30", lines.Patient[2]);
        Assert.Equal(2, lines.Items.Count);
        Assert.Equal("CBC", lines.Items[0].TestCode);
        Assert.Equal("صورة دم كاملة", lines.Items[0].TestName);
        Assert.Equal("100", lines.BarcodePayload);
        Assert.Null(lines.EmptyNotice);
    }

    [Fact]
    public void BuildTextLines_PatientIdShown_WhenLabIdMissing()
    {
        var order = new LabOrderDto(
            7, "أحمد محمد علي", null, "7",
            new DateTime(2026, 10, 1, 9, 30, 0, DateTimeKind.Utc),
            new List<LabOrderLineDto> { new("CBC", "صورة دم كاملة") });

        var lines = LabOrderPdfWriter.BuildTextLines(order, LabText());

        Assert.Contains("7", lines.Patient[1]);
        Assert.Equal("7", lines.BarcodePayload);
    }

    [Fact]
    public void BuildTextLines_EmptyOrder_RendersNoTestsNoticeWithBarcode()
    {
        var lines = LabOrderPdfWriter.BuildTextLines(Order(), LabText());

        Assert.Empty(lines.Items);
        Assert.Equal("لا تحاليل مطلوبة", lines.EmptyNotice);
        Assert.Equal("100", lines.BarcodePayload);
        Assert.Contains("أحمد محمد علي", lines.Patient[0]);
    }

    [Fact]
    public void BuildTextLines_NullOrder_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => LabOrderPdfWriter.BuildTextLines(null!, LabText()));
    }
}
