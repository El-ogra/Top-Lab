using System.Text;
using TopLab.Application.Features.Statistics.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// R-F05 (VG-02) — <c>BandedResultMonitorPdfWriter</c>.
///
/// Binding rules checked here:
///   1. The writer sets <c>QuestPDF.Settings.License</c> in its OWN static constructor,
///      so this file can be run under <c>--filter</c> in isolation without any other
///      writer's static initialiser having run first.
///   2. A PDF test must assert REAL output: the file exists, is non-empty, starts with
///      the %PDF header and ends with %%EOF.
///   3. It never overwrites (BR-F05-14) and rejects a missing directory.
/// </summary>
public class BandedResultMonitorPdfWriterTests
{
    private static LabPrintTextDto LabText() => new(
        LabName: "معمل النور للتحاليل",
        Address: "القاهرة",
        Phone: "0100000000",
        FontFamily: "Arial",
        FontSizePt: 11);

    private static BandedResultRowDto Row(
        int id,
        DateTime enteredAtUtc,
        int patientId,
        string name,
        string sex = "Male",
        int age = 30,
        string ageUnit = "Year",
        string referral = "بدون جهة إحالة",
        string result = "5.5",
        string status = "معتمد") => new(
        id,
        enteredAtUtc,
        patientId,
        name,
        sex,
        age,
        ageUnit,
        referral,
        "سكر صائم",
        result,
        status);

    private static BandedResultMonitorDto Monitor(params BandedResultRowDto[] rows) => new(
        From: new DateOnly(2026, 3, 1),
        To: new DateOnly(2026, 3, 31),
        TestId: 10,
        TestName: "سكر صائم",
        MinValue: 3m,
        MaxValue: 7m,
        Rows: rows,
        TotalCount: rows.Length);

    private static string TempPath(string tag) =>
        Path.Combine(Path.GetTempPath(), $"toplab-banded-{tag}-{Guid.NewGuid():N}.pdf");

    private static readonly DateTime Mar5 = new(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc);

    // =====================================================================
    // Real output — not just "no throw"
    // =====================================================================

    [Fact]
    public async Task ProducesNonEmptyPdf()
    {
        var writer = new BandedResultMonitorPdfWriter();
        var path = TempPath("nonempty");

        try
        {
            await writer.WritePdfAsync(path, Monitor(Row(1, Mar5, 1, "أحمد محمد")), LabText());

            Assert.True(File.Exists(path), "the writer must actually create the file");
            var bytes = await File.ReadAllBytesAsync(path);
            Assert.True(bytes.Length > 0, "the produced PDF must not be empty");
            Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));

            var tail = Encoding.ASCII.GetString(bytes, Math.Max(0, bytes.Length - 32), Math.Min(32, bytes.Length));
            Assert.Contains("%%EOF", tail, StringComparison.Ordinal);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void BuildTextLines_YieldsLabHeaderAndCriteriaLine()
    {
        var lines = BandedResultMonitorPdfWriter.BuildTextLines(Monitor(), LabText());

        Assert.Equal("معمل النور للتحاليل", lines.Header[0]);
        Assert.Equal("القاهرة", lines.Header[1]);
        Assert.Equal("0100000000", lines.Header[2]);

        // The criteria line names the test, the band and the period.
        Assert.Contains("سكر صائم", lines.CriteriaLine, StringComparison.Ordinal);
        Assert.Contains("3", lines.CriteriaLine, StringComparison.Ordinal);
        Assert.Contains("7", lines.CriteriaLine, StringComparison.Ordinal);
        Assert.Contains("2026/03/01", lines.CriteriaLine, StringComparison.Ordinal);
        Assert.Contains("2026/03/31", lines.CriteriaLine, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildTextLines_YieldsTheNineGridHeaders()
    {
        var lines = BandedResultMonitorPdfWriter.BuildTextLines(Monitor(), LabText());

        Assert.Equal("التاريخ", lines.Headers.Date);
        Assert.Equal("المريض", lines.Headers.Patient);
        Assert.Equal("الرقم", lines.Headers.PatientNumber);
        Assert.Equal("الجنس", lines.Headers.Sex);
        Assert.Equal("العمر", lines.Headers.Age);
        Assert.Equal("جهة الإحالة", lines.Headers.ReferralEntity);
        Assert.Equal("التحليل", lines.Headers.Test);
        Assert.Equal("النتيجة", lines.Headers.Result);
        Assert.Equal("الحالة", lines.Headers.Status);
    }

    [Fact]
    public void BuildTextLines_MapsEveryRow()
    {
        var lines = BandedResultMonitorPdfWriter.BuildTextLines(
            Monitor(Row(1, Mar5, 42, "أحمد محمد", "Female", 27, "Month", "مستشفى النور", "3.25", "تمت الطباعة")),
            LabText());

        var row = Assert.Single(lines.Rows);
        Assert.Equal("2026/03/05 12:00", row.Date);
        Assert.Equal("أحمد محمد", row.PatientFullName);
        Assert.Equal("42", row.PatientNumber);
        Assert.Equal("Female", row.Sex);
        Assert.Equal("27 Month", row.Age);
        Assert.Equal("مستشفى النور", row.ReferralEntityName);
        Assert.Equal("سكر صائم", row.TestName);
        Assert.Equal("3.25", row.ResultValue);
        Assert.Equal("تمت الطباعة", row.StatusText);

        Assert.Equal("عدد النتائج: 1", lines.TotalLine);
    }

    [Fact]
    public void BuildTextLines_EmptyRows_StillYieldsHeaderAndCriteria()
    {
        var lines = BandedResultMonitorPdfWriter.BuildTextLines(Monitor(), LabText());

        Assert.NotEmpty(lines.Header);
        Assert.NotEmpty(lines.CriteriaLine);
        Assert.Empty(lines.Rows);
        Assert.Equal("عدد النتائج: 0", lines.TotalLine);
    }

    [Fact]
    public void BuildTextLines_EmptyLabNameOmitsHeaderLines()
    {
        var noText = new LabPrintTextDto(string.Empty, string.Empty, string.Empty, "Arial", 11);

        var lines = BandedResultMonitorPdfWriter.BuildTextLines(Monitor(), noText);

        Assert.Empty(lines.Header);
        Assert.NotEmpty(lines.CriteriaLine);
    }

    [Fact]
    public async Task RenderedPdfIsLargerThanAnEmptyGrid()
    {
        // Proves the rows were actually laid out: three rows must produce a materially
        // larger file than the same document with no rows.
        var writer = new BandedResultMonitorPdfWriter();
        var withRows = TempPath("with");
        var withoutRows = TempPath("without");

        try
        {
            await writer.WritePdfAsync(withRows, Monitor(
                Row(1, Mar5, 1, "أحمد محمد"),
                Row(2, Mar5, 2, "سارة علي"),
                Row(3, Mar5, 3, "محمد حسن")), LabText());
            await writer.WritePdfAsync(withoutRows, Monitor(), LabText());

            var withBytes = (await File.ReadAllBytesAsync(withRows)).Length;
            var withoutBytes = (await File.ReadAllBytesAsync(withoutRows)).Length;

            Assert.True(withBytes > withoutBytes,
                $"a 3-row grid ({withBytes} bytes) must exceed an empty grid ({withoutBytes} bytes)");
        }
        finally
        {
            if (File.Exists(withRows))
            {
                File.Delete(withRows);
            }

            if (File.Exists(withoutRows))
            {
                File.Delete(withoutRows);
            }
        }
    }

    // =====================================================================
    // Argument validation
    // =====================================================================

    [Fact]
    public async Task RejectsEmptyPath()
    {
        var writer = new BandedResultMonitorPdfWriter();

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => writer.WritePdfAsync(string.Empty, Monitor(), LabText()));
        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => writer.WritePdfAsync("   ", Monitor(), LabText()));
    }

    [Fact]
    public async Task RejectsNullArguments()
    {
        var writer = new BandedResultMonitorPdfWriter();
        var path = TempPath("null");

        // WritePdfAsync throws synchronously but returns Task, so ThrowsAsync is required.
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => writer.WritePdfAsync(path, null!, LabText()));
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => writer.WritePdfAsync(path, Monitor(), null!));

        Assert.Throws<ArgumentNullException>(
            () => BandedResultMonitorPdfWriter.BuildTextLines(null!, LabText()));
        Assert.Throws<ArgumentNullException>(
            () => BandedResultMonitorPdfWriter.BuildTextLines(Monitor(), null!));
    }

    // =====================================================================
    // BR-F05-14: never overwrite
    // =====================================================================

    [Fact]
    public async Task NeverOverwritesAnExistingFile()
    {
        var writer = new BandedResultMonitorPdfWriter();
        var path = TempPath("overwrite");
        await File.WriteAllBytesAsync(path, new byte[] { 1, 2, 3 });

        try
        {
            await Assert.ThrowsAsync<IOException>(
                () => writer.WritePdfAsync(path, Monitor(Row(1, Mar5, 1, "أحمد محمد")), LabText()));
            Assert.Equal(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(path));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task RejectsAMissingDirectory()
    {
        var writer = new BandedResultMonitorPdfWriter();
        var path = Path.Combine(Path.GetTempPath(), $"toplab-banded-nodir-{Guid.NewGuid():N}", "x.pdf");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => writer.WritePdfAsync(path, Monitor(), LabText()));
    }
}