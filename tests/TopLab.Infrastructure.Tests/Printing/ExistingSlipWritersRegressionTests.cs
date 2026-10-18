using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// Phase 6 (GAP-013) regression guard.
/// The group-patient report slip is a NEW document; it must not have altered the
/// output of the neighbouring slip/bench writers it reuses conventions from.
/// These assertions pin the exact patient-block and header strings those writers
/// produced before Phase 6, so any accidental edit to them fails here.
/// </summary>
public class ExistingSlipWritersRegressionTests
{
    private static LabPrintTextDto LabText()
        => new("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12);

    [Fact]
    public void LabOrderSlip_OutputIsUnchanged()
    {
        var order = new LabOrderDto(
            7,
            "أحمد محمد علي",
            "100",
            "100",
            new DateTime(2026, 10, 1, 9, 30, 0, DateTimeKind.Utc),
            new List<LabOrderLineDto> { new("CBC", "صورة دم كاملة") });

        var lines = LabOrderPdfWriter.BuildTextLines(order, LabText());

        Assert.Equal(new[] { "مختبر الشفاء", "شارع الجمهورية", "01000000000" }, lines.Header);
        Assert.Equal(
            new[] { "المريض: أحمد محمد علي", "رقم المعمل: 100", "التاريخ: 2026-10-01 09:30" },
            lines.Patient);
        Assert.Equal("CBC", Assert.Single(lines.Items).TestCode);
        Assert.Equal("100", lines.BarcodePayload);
        Assert.Null(lines.EmptyNotice);
    }

    [Fact]
    public void LabOrderSlip_EmptyOrderNoticeIsUnchanged()
    {
        var order = new LabOrderDto(
            7, "أحمد", null, "7",
            new DateTime(2026, 10, 1, 9, 30, 0, DateTimeKind.Utc),
            new List<LabOrderLineDto>());

        var lines = LabOrderPdfWriter.BuildTextLines(order, LabText());

        Assert.Equal("لا تحاليل مطلوبة", lines.EmptyNotice);
        Assert.Contains("رقم المريض: 7", lines.Patient);
    }

    [Fact]
    public void VisitWorkSheet_OutputIsUnchangedAndDistinctFromTheGroupSlip()
    {
        var sheet = new VisitWorkSheetDto(
            7,
            "أحمد محمد علي",
            "100",
            new List<WorkSheetSectionDto>
            {
                new(1, "الكيمياء", new List<WorkSheetLineDto>
                {
                    new(100, 7, "أحمد محمد علي", "100", "Glucose", "GLU", "GLU-100",
                        false, null, false, false, 0)
                })
            },
            new List<VisitWorkSheetSampleDto> { new(100, false, false, true, false, false, false) },
            1,
            false,
            false,
            true);

        var lines = WorkSheetPdfWriter.BuildTextLines(sheet, LabText());

        // The bench worksheet keeps its own shape: sections with barcode/sample/draw
        // columns and a total line — not the slip's patient block + test table.
        Assert.Equal(new[] { "المريض: أحمد محمد علي", "رقم المعمل: 100" }, lines.Patient);
        Assert.Equal("إجمالي التحاليل: 1", lines.TotalLine);
        var section = Assert.Single(lines.Sections);
        Assert.Equal("الكيمياء", section.Name);
        var row = Assert.Single(section.Rows);
        Assert.Equal("Glucose", row.TestName);
        Assert.Equal("GLU-100", row.Barcode);
        Assert.Equal("B", row.SampleKinds);
        Assert.Equal("[ ]", row.Drawn);
    }
}
