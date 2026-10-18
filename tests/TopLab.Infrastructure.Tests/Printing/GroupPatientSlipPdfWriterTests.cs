using System.Text;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// Phase 6 (GAP-013): group-patient report slip writer.
/// Proves one slip per patient, the correct identity on each slip, page
/// separation, and safe empty-input behaviour — both at the pure content-mapping
/// level and against the real rendered PDF.
/// </summary>
public class GroupPatientSlipPdfWriterTests
{
    private static LabPrintTextDto LabText()
    {
        return new LabPrintTextDto("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12);
    }

    private static GroupPatientSlipDto Slip(int patientId, string name, string labId, params GroupPatientSlipLineDto[] lines)
    {
        return new GroupPatientSlipDto(
            patientId,
            name,
            labId,
            labId,
            new DateTime(2026, 10, 1, 9, 30, 0, DateTimeKind.Utc),
            lines.ToList());
    }

    private static string TempPath()
        => Path.Combine(Path.GetTempPath(), $"toplab-groupslip-{Guid.NewGuid():N}.pdf");

    /// <summary>Counts PDF page objects; skips the /Pages container node.</summary>
    private static int CountPages(string latinPdf)
    {
        var count = 0;
        var idx = 0;
        while ((idx = latinPdf.IndexOf("/Type /Page", idx, StringComparison.Ordinal)) >= 0)
        {
            if (!latinPdf.AsSpan(idx).StartsWith("/Type /Pages", StringComparison.Ordinal))
            {
                count++;
            }

            idx += 10;
        }

        return count;
    }

    private static string PdfLatin(string path)
        => Encoding.Latin1.GetString(File.ReadAllBytes(path));

    [Fact]
    public void BuildTextPages_OnePagePerSelectedPatient_WithItsOwnIdentityAndTests()
    {
        var batch = new GroupPatientSlipBatchDto(new List<GroupPatientSlipDto>
        {
            Slip(1, "أحمد محمد", "100", new GroupPatientSlipLineDto("CBC", "صورة دم كاملة")),
            Slip(2, "سيد علي", "200", new GroupPatientSlipLineDto("GLU", "Glucose"))
        });

        var pages = GroupPatientSlipPdfWriter.BuildTextPages(batch, LabText());

        Assert.Equal(2, pages.Count);

        // Each page is self-describing: its own lab header, patient block and tests.
        Assert.All(pages, p => Assert.Contains("مختبر الشفاء", p.Header));

        Assert.Contains("أحمد محمد", pages[0].Patient[0]);
        Assert.Contains("100", pages[0].Patient[1]);
        Assert.Equal("CBC", Assert.Single(pages[0].Rows).TestCode);
        Assert.Equal("100", pages[0].Identifier);
        Assert.Null(pages[0].EmptyNotice);

        Assert.Contains("سيد علي", pages[1].Patient[0]);
        Assert.Contains("200", pages[1].Patient[1]);
        Assert.Equal("GLU", Assert.Single(pages[1].Rows).TestCode);
        Assert.Equal("200", pages[1].Identifier);

        // No bleed: patient 1's name never appears on patient 2's page.
        Assert.DoesNotContain(pages[1].Patient, l => l.Contains("أحمد محمد", StringComparison.Ordinal));
        Assert.DoesNotContain(pages[0].Patient, l => l.Contains("سيد علي", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildTextPages_PatientWithoutTests_CarriesIdentityPlusNoTestsNotice()
    {
        var batch = new GroupPatientSlipBatchDto(new List<GroupPatientSlipDto>
        {
            Slip(1, "أحمد محمد", "100")
        });

        var pages = GroupPatientSlipPdfWriter.BuildTextPages(batch, LabText());

        var page = Assert.Single(pages);
        Assert.Empty(page.Rows);
        Assert.Equal("لا تحاليل مطلوبة", page.EmptyNotice);
        Assert.Contains("أحمد محمد", page.Patient[0]);
        Assert.Equal("100", page.Identifier);
    }

    [Fact]
    public void BuildTextPages_EmptySelection_ProducesOneSafeNoticePage()
    {
        var pages = GroupPatientSlipPdfWriter.BuildTextPages(
            new GroupPatientSlipBatchDto(new List<GroupPatientSlipDto>()), LabText());

        var page = Assert.Single(pages);
        Assert.Equal("لا يوجد مرضى محددون للطباعة", page.EmptyNotice);
        Assert.Empty(page.Patient);
        Assert.Empty(page.Rows);
        Assert.Empty(page.Identifier);
        Assert.Contains("مختبر الشفاء", page.Header);
    }

    [Fact]
    public void BuildTextPages_NullBatch_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => GroupPatientSlipPdfWriter.BuildTextPages(null!, LabText()));
    }

    [Fact]
    public void BuildTextPages_NullLabText_Throws()
    {
        var batch = new GroupPatientSlipBatchDto(new List<GroupPatientSlipDto> { Slip(1, "أحمد", "100") });

        Assert.Throws<ArgumentNullException>(() => GroupPatientSlipPdfWriter.BuildTextPages(batch, null!));
    }

    [Fact]
    public async Task WritePdfAsync_OnePagePerPatient_WithSeparatedIdentity()
    {
        var batch = new GroupPatientSlipBatchDto(new List<GroupPatientSlipDto>
        {
            Slip(1, "أحمد محمد", "100", new GroupPatientSlipLineDto("CBC", "صورة دم كاملة")),
            Slip(2, "سيد علي", "200", new GroupPatientSlipLineDto("GLU", "Glucose")),
            Slip(3, "منى حسن", "300", new GroupPatientSlipLineDto("ESR", "ESR"))
        });

        var path = TempPath();
        try
        {
            var writer = new GroupPatientSlipPdfWriter(new TopLab.Infrastructure.Barcode.BarcodeLabelRenderer());
            await writer.WritePdfAsync(path, batch, LabText(), CancellationToken.None);

            Assert.True(File.Exists(path));
            var bytes = File.ReadAllBytes(path);
            Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));

            // Three patients ⇒ three independently archivable pages.
            Assert.Equal(3, CountPages(PdfLatin(path)));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task WritePdfAsync_SinglePatient_ProducesExactlyOnePage()
    {
        var batch = new GroupPatientSlipBatchDto(new List<GroupPatientSlipDto>
        {
            Slip(7, "أحمد محمد", "100", new GroupPatientSlipLineDto("CBC", "صورة دم كاملة"))
        });

        var path = TempPath();
        try
        {
            var writer = new GroupPatientSlipPdfWriter(new TopLab.Infrastructure.Barcode.BarcodeLabelRenderer());
            await writer.WritePdfAsync(path, batch, LabText(), CancellationToken.None);

            Assert.Equal(1, CountPages(PdfLatin(path)));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task WritePdfAsync_EmptyBatch_StillWritesAValidSinglePageDocument()
    {
        var path = TempPath();
        try
        {
            var writer = new GroupPatientSlipPdfWriter(new TopLab.Infrastructure.Barcode.BarcodeLabelRenderer());
            await writer.WritePdfAsync(
                path, new GroupPatientSlipBatchDto(new List<GroupPatientSlipDto>()), LabText(), CancellationToken.None);

            var bytes = File.ReadAllBytes(path);
            Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
            Assert.Equal(1, CountPages(PdfLatin(path)));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task WritePdfAsync_ExistingFile_ThrowsAndDoesNotOverwrite()
    {
        var path = TempPath();
        await File.WriteAllTextAsync(path, "sentinel");
        try
        {
            var writer = new GroupPatientSlipPdfWriter(new TopLab.Infrastructure.Barcode.BarcodeLabelRenderer());
            var batch = new GroupPatientSlipBatchDto(new List<GroupPatientSlipDto> { Slip(1, "أحمد", "100") });

            await Assert.ThrowsAsync<IOException>(
                () => writer.WritePdfAsync(path, batch, LabText(), CancellationToken.None));

            Assert.Equal("sentinel", await File.ReadAllTextAsync(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task WritePdfAsync_MissingDirectory_Throws()
    {
        var writer = new GroupPatientSlipPdfWriter(new TopLab.Infrastructure.Barcode.BarcodeLabelRenderer());
        var batch = new GroupPatientSlipBatchDto(new List<GroupPatientSlipDto> { Slip(1, "أحمد", "100") });
        var missing = Path.Combine(Path.GetTempPath(), $"toplab-missing-{Guid.NewGuid():N}", "slip.pdf");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => writer.WritePdfAsync(missing, batch, LabText(), CancellationToken.None));
    }
}
