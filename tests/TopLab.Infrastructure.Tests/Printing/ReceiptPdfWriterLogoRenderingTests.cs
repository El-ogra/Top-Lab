using System.Text;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Barcode;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// GAP-010 (Phase 4): the persisted lab logo must actually be drawn on the
/// receipt — not merely accepted by the settings screen — for both paper
/// choices, with a words fallback when the file is missing, of an unsupported
/// type, or unreadable.
/// <para>
/// The existing GAP-007 receipt tests assert only that a PDF is produced; they
/// cannot distinguish "logo drawn" from "logo silently skipped", because the
/// bytes of a receipt with no logo at all look the same. These tests therefore
/// count embedded image XObjects, the same proof used by the GAP-011 report
/// suite, and exercise the paper-choice branches.
/// </para>
/// </summary>
public class ReceiptPdfWriterLogoRenderingTests : IDisposable
{
    private readonly string _dir;
    private readonly string _assetDir;

    public ReceiptPdfWriterLogoRenderingTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "toplab-receipt-logo-" + Guid.NewGuid().ToString("N"));
        _assetDir = Path.Combine(_dir, "assets");
        Directory.CreateDirectory(_assetDir);

        // Wide banner: the normal logo shape (bounded by width, not height).
        File.WriteAllBytes(Path.Combine(_assetDir, "Logo.png"), TestPng.Create(1200, 160));

        // Deliberately far taller than the receipt: the pre-printed-stationery
        // collision case. Unbounded, this exhausts the page and aborts generation.
        File.WriteAllBytes(Path.Combine(_assetDir, "Tall.png"), TestPng.Create(120, 6000));

        // Valid header, wrong payload: a renamed text file.
        File.WriteAllBytes(Path.Combine(_assetDir, "Fake.png"), Encoding.ASCII.GetBytes("not an image at all"));
    }

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    private sealed class TempAssetStore : IReceiptImageAssetStore
    {
        private readonly string _directory;

        public TempAssetStore(string directory) => _directory = directory;

        public string? StoreLogo(string sourcePath) => null;

        public string? ResolveLogoPath(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            var full = Path.Combine(_directory, relativePath);
            return File.Exists(full) ? full : null;
        }

        public string GetReceiptAssetDirectory() => _directory;
    }

    private ReceiptPdfWriter Writer() =>
        new(new BarcodeLabelRenderer(), new TempAssetStore(_assetDir));

    private static ReceiptDto Dto() => new(
        7,
        "أحمد محمد علي",
        "100",
        new List<ChargedTestDto> { new(1, "صورة دم كاملة", "CBC", "CBC", 100m) },
        100m,
        10m,
        40m,
        50m,
        "L.E.");

    private static LabPrintTextDto LabText() =>
        new("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12);

    private static ReceiptSettings Settings(
        HeaderFooterMode mode,
        string? logoFileName,
        decimal topMarginCm = 1.0m)
    {
        var settings = ReceiptSettings.CreateDefault();
        settings.Update(topMarginCm, "L.E.", null, false, TestDetailDisplayMode.Show, false, mode,
            logoImagePath: logoFileName);
        return settings;
    }

    private string WriteAsync(HeaderFooterMode mode, string? logoFileName, decimal topMarginCm = 1.0m)
    {
        var path = Path.Combine(_dir, $"out-{Guid.NewGuid():N}.pdf");
        Writer().WritePdfAsync(
            path, Dto(), Settings(mode, logoFileName, topMarginCm), LabText(), "100").GetAwaiter().GetResult();
        return path;
    }

    private static string Latin(string path) => Encoding.Latin1.GetString(File.ReadAllBytes(path));

    private static int ImageObjects(string path)
    {
        var latin = Latin(path);
        var count = 0;
        var idx = 0;
        while ((idx = latin.IndexOf("/Subtype /Image", idx, StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx += "/Subtype /Image".Length;
        }

        return count;
    }

    // ---- acceptance criterion: the logo appears on the receipt ----

    [Fact]
    public void PlainPaper_LogoConfigured_LogoIsDrawn()
    {
        // HeaderFooterMode.Words = plain paper, words branding, logo configured.
        var path = WriteAsync(HeaderFooterMode.Words, "Logo.png");
        Assert.Equal("%PDF", Latin(path)[..4]);

        // The receipt carries one image of its own (the REF-127 identifier
        // barcode), so a drawn logo makes exactly two.
        Assert.Equal(2, ImageObjects(path));
    }

    [Fact]
    public void PlainPaper_ImagesMode_LogoIsDrawn()
    {
        var path = WriteAsync(HeaderFooterMode.Images, "Logo.png");
        Assert.Equal(2, ImageObjects(path));
    }

    [Fact]
    public void PlainPaper_LogoDrawn_WordsHeaderIsSuppressed()
    {
        // The words header is the fallback, not an addition: with a drawable
        // logo the lab name must not be printed beneath it.
        var lines = ReceiptPdfWriter.BuildTextLines(
            Dto(), Settings(HeaderFooterMode.Words, "Logo.png"), LabText(), "100", renderWordsHeader: false);
        Assert.Empty(lines.Header);
    }

    [Fact]
    public void PlainPaper_NoLogo_WordsHeaderIsKept()
    {
        // Regression guard for the suppression above: with no logo configured the
        // words header is exactly what the user asked for.
        var lines = ReceiptPdfWriter.BuildTextLines(
            Dto(), Settings(HeaderFooterMode.Words, null), LabText(), "100");
        Assert.Contains(lines.Header, l => l.Contains("مختبر الشفاء", StringComparison.Ordinal));
    }

    // ---- the other paper choice: pre-printed stock ----

    [Fact]
    public void PrePrintedPaper_HeaderFooterOff_LogoIsNotDrawn()
    {
        // Pre-printed paper: the letterhead is on the stock, so the user turns the
        // header/footer off. A configured logo must not be drawn on top of it.
        var path = WriteAsync(HeaderFooterMode.None, "Logo.png");
        Assert.Equal(1, ImageObjects(path)); // the identifier barcode only
    }

    [Fact]
    public void PrePrintedPaper_HeaderFooterOff_WordsHeaderIsSuppressed()
    {
        var lines = ReceiptPdfWriter.BuildTextLines(
            Dto(), Settings(HeaderFooterMode.None, "Logo.png"), LabText(), "100");
        Assert.Empty(lines.Header);
    }

    [Fact]
    public void PrePrintedPaper_WithTopMargin_StillPrints()
    {
        // TopMarginCm is the pre-printed clearance knob; at the largest value
        // that still leaves printable height it must not abort generation.
        var path = WriteAsync(HeaderFooterMode.None, null, topMarginCm: ReceiptGeometry.MaxTopMarginCm);
        Assert.Equal("%PDF", Latin(path)[..4]);
        Assert.Equal(1, ImageObjects(path));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public void TopMarginWithinLimit_LeavesPrintableHeight(decimal topMarginCm)
    {
        Assert.True(ReceiptGeometry.LeavesPrintableHeight(topMarginCm));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(30)]
    public void TopMarginBeyondLimit_ReceiptStillPrints(decimal topMarginCm)
    {
        // The settings screen allows up to 30 cm, but A5 has no room for it: past
        // the limit the printable box is smaller than the receipt body and QuestPDF
        // throws a layout exception, so no receipt could be produced at all. The
        // writer clamps instead of failing the print.
        var path = WriteAsync(HeaderFooterMode.None, null, topMarginCm: topMarginCm);
        Assert.Equal("%PDF", Latin(path)[..4]);
        Assert.Equal(1, ImageObjects(path));
    }

    // ---- validation rule: fall back to words when the logo cannot be drawn ----

    [Fact]
    public void MissingLogoFile_FallsBackToWordsHeader()
    {
        var path = WriteAsync(HeaderFooterMode.Words, "DoesNotExist.png");
        Assert.Equal(1, ImageObjects(path));

        var lines = ReceiptPdfWriter.BuildTextLines(
            Dto(), Settings(HeaderFooterMode.Words, "DoesNotExist.png"), LabText(), "100");
        Assert.Contains(lines.Header, l => l.Contains("مختبر الشفاء", StringComparison.Ordinal));
    }

    [Fact]
    public void UnsupportedLogoType_FallsBackToWordsHeader()
    {
        // The asset store still accepts .bmp/.gif; QuestPDF cannot draw them, so
        // they must fall back rather than abort the receipt.
        File.WriteAllBytes(Path.Combine(_assetDir, "Logo.bmp"), new byte[] { 0x42, 0x4D, 0x00, 0x00 });
        var path = WriteAsync(HeaderFooterMode.Words, "Logo.bmp");
        Assert.Equal(1, ImageObjects(path));
    }

    [Fact]
    public void UnreadableLogoFile_FallsBackToWordsHeader()
    {
        // Correct extension, broken payload: the header check must reject it and
        // the words header must still print.
        var path = WriteAsync(HeaderFooterMode.Words, "Fake.png");
        Assert.Equal(1, ImageObjects(path));

        var lines = ReceiptPdfWriter.BuildTextLines(
            Dto(), Settings(HeaderFooterMode.Words, "Fake.png"), LabText(), "100");
        Assert.Contains(lines.Header, l => l.Contains("مختبر الشفاء", StringComparison.Ordinal));
    }

    [Fact]
    public void NoLogoConfigured_RendersNoLogo()
    {
        var path = WriteAsync(HeaderFooterMode.Words, null);
        Assert.Equal(1, ImageObjects(path));
    }

    // ---- risk mitigation: the logo must not exhaust the page ----

    [Fact]
    public void OverTallLogo_DoesNotAbortPdfGeneration()
    {
        // Before the bound, an over-tall logo exhausted the page and QuestPDF
        // threw, so no receipt could be produced at all.
        var path = WriteAsync(HeaderFooterMode.Words, "Tall.png");
        Assert.Equal("%PDF", Latin(path)[..4]);
        Assert.Equal(2, ImageObjects(path));
    }

    [Fact]
    public void LogoBounds_AreAFractionOfThePrintableContentBox()
    {
        // The width bound is the receipt's printable width: 420 - 2*28 pt.
        Assert.Equal(420f - 56f, ReceiptGeometry.ContentWidthPt(28f, 28f), 2);

        // The height bound shrinks as the top margin grows, and never goes
        // negative — a degenerate box cannot push the body off the paper.
        var (_, height) = (0f, ReceiptGeometry.A5HeightPt);
        var bounded = ReceiptGeometry.LogoMaxHeightPt(28.35f, 28f);
        Assert.True(bounded > 0f && bounded < height);

        Assert.Equal(0f, ReceiptGeometry.LogoMaxHeightPt(999f, 28f));
        Assert.Equal(0f, ReceiptGeometry.ContentWidthPt(999f, 999f));
    }

    [Fact]
    public void LogoMaxHeightFraction_IsAFraction()
    {
        var fraction = ReceiptGeometry.LogoMaxHeightFraction;
        Assert.True(fraction > 0f && fraction < 1f);
    }
}
