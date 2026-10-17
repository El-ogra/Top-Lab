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
/// GAP-007: tests for receipt logo rendering, header/footer font family/size/colour,
/// and regression (no-logo output unchanged).
/// </summary>
public class ReceiptPdfWriterFontLogoTests
{
    private static ReceiptDto Dto()
    {
        return new ReceiptDto(
            7,
            "أحمد محمد علي",
            "100",
            new List<ChargedTestDto>
            {
                new(1, "صورة دم كاملة", "CBC", "CBC", 100m),
                new(2, "Glucose", "GLU", "Glucose", 50m)
            },
            150m,
            15m,
            50m,
            85m,
            "L.E.");
    }

    private static LabPrintTextDto LabText()
    {
        return new LabPrintTextDto("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12);
    }

    private static ReceiptSettings SettingsWithLogoAndFonts(string logoPath)
    {
        var settings = ReceiptSettings.CreateDefault();
        settings.Update(1m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.Images,
            headerFontFamily: "Arial", headerFontSizePt: 14m, headerFontColor: "#FF0000",
            footerFontFamily: "Tahoma", footerFontSizePt: 10m, footerFontColor: "#0000FF",
            logoImagePath: logoPath);
        return settings;
    }

    private static ReceiptSettings SettingsDefault()
    {
        var settings = ReceiptSettings.CreateDefault();
        settings.Update(1m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.Words);
        return settings;
    }

    private static ReceiptImageAssetStore CreateStoreWithLogo(out string logoFileName)
    {
        var store = new ReceiptImageAssetStore();
        var tempDir = Path.Combine(Path.GetTempPath(), "toplab-receipt-pdf-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var sourcePath = Path.Combine(tempDir, "logo.png");
            // Write a minimal valid PNG (1x1 pixel)
            File.WriteAllBytes(sourcePath, new byte[]
            {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, // IHDR chunk
                0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, // 1x1
                0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, 0xDE,
                0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, 0x54, // IDAT chunk
                0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00, 0x00,
                0x00, 0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC, 0x33,
                0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, // IEND chunk
                0xAE, 0x42, 0x60, 0x82
            });

            logoFileName = store.StoreLogo(sourcePath)!;
            return store;
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task WritePdfAsync_WithLogoAndFonts_GeneratesPdf()
    {
        var store = CreateStoreWithLogo(out var logoFileName);
        var settings = SettingsWithLogoAndFonts(logoFileName);
        var writer = new ReceiptPdfWriter(new BarcodeLabelRenderer(), store);

        var pdfPath = Path.Combine(Path.GetTempPath(), "receipt-logo-test-" + Guid.NewGuid().ToString("N") + ".pdf");
        try
        {
            await writer.WritePdfAsync(pdfPath, Dto(), settings, LabText(), "100");

            Assert.True(File.Exists(pdfPath));
            var bytes = File.ReadAllBytes(pdfPath);
            Assert.StartsWith("%PDF", Encoding.ASCII.GetString(bytes));
            Assert.True(bytes.Length > 100);
        }
        finally
        {
            if (File.Exists(pdfPath)) File.Delete(pdfPath);
        }
    }

    [Fact]
    public async Task WritePdfAsync_WithoutLogo_GeneratesPdf()
    {
        var settings = SettingsDefault();
        var writer = new ReceiptPdfWriter(new BarcodeLabelRenderer());

        var pdfPath = Path.Combine(Path.GetTempPath(), "receipt-nologo-test-" + Guid.NewGuid().ToString("N") + ".pdf");
        try
        {
            await writer.WritePdfAsync(pdfPath, Dto(), settings, LabText(), "100");

            Assert.True(File.Exists(pdfPath));
            var bytes = File.ReadAllBytes(pdfPath);
            Assert.StartsWith("%PDF", Encoding.ASCII.GetString(bytes));
            Assert.True(bytes.Length > 100);
        }
        finally
        {
            if (File.Exists(pdfPath)) File.Delete(pdfPath);
        }
    }

    [Fact]
    public async Task WritePdfAsync_WithLogoPathButMissingFile_StillGeneratesPdf()
    {
        // Logo path configured but file doesn't exist — should gracefully skip
        var settings = SettingsWithLogoAndFonts("NonExistentLogo.png");
        var writer = new ReceiptPdfWriter(new BarcodeLabelRenderer());

        var pdfPath = Path.Combine(Path.GetTempPath(), "receipt-missinglogo-test-" + Guid.NewGuid().ToString("N") + ".pdf");
        try
        {
            await writer.WritePdfAsync(pdfPath, Dto(), settings, LabText(), "100");

            Assert.True(File.Exists(pdfPath));
            var bytes = File.ReadAllBytes(pdfPath);
            Assert.StartsWith("%PDF", Encoding.ASCII.GetString(bytes));
        }
        finally
        {
            if (File.Exists(pdfPath)) File.Delete(pdfPath);
        }
    }

    [Fact]
    public async Task WritePdfAsync_WithNullLogoPath_GeneratesPdf()
    {
        var settings = ReceiptSettings.CreateDefault();
        settings.Update(1m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.Words,
            logoImagePath: null);
        var writer = new ReceiptPdfWriter(new BarcodeLabelRenderer());

        var pdfPath = Path.Combine(Path.GetTempPath(), "receipt-nullogo-test-" + Guid.NewGuid().ToString("N") + ".pdf");
        try
        {
            await writer.WritePdfAsync(pdfPath, Dto(), settings, LabText(), "100");

            Assert.True(File.Exists(pdfPath));
            var bytes = File.ReadAllBytes(pdfPath);
            Assert.StartsWith("%PDF", Encoding.ASCII.GetString(bytes));
        }
        finally
        {
            if (File.Exists(pdfPath)) File.Delete(pdfPath);
        }
    }

    [Fact]
    public async Task WritePdfAsync_WithCustomFontsOnly_GeneratesPdf()
    {
        var settings = ReceiptSettings.CreateDefault();
        settings.Update(1m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.Images,
            headerFontFamily: "Tahoma", headerFontSizePt: 16m, headerFontColor: "#00FF00",
            footerFontFamily: "Calibri", footerFontSizePt: 8m, footerFontColor: "#000000");
        var writer = new ReceiptPdfWriter(new BarcodeLabelRenderer());

        var pdfPath = Path.Combine(Path.GetTempPath(), "receipt-fonts-test-" + Guid.NewGuid().ToString("N") + ".pdf");
        try
        {
            await writer.WritePdfAsync(pdfPath, Dto(), settings, LabText(), "100");

            Assert.True(File.Exists(pdfPath));
            var bytes = File.ReadAllBytes(pdfPath);
            Assert.StartsWith("%PDF", Encoding.ASCII.GetString(bytes));
        }
        finally
        {
            if (File.Exists(pdfPath)) File.Delete(pdfPath);
        }
    }

    [Fact]
    public async Task WritePdfAsync_FileAlreadyExists_ThrowsIOException()
    {
        var settings = SettingsDefault();
        var writer = new ReceiptPdfWriter(new BarcodeLabelRenderer());

        var pdfPath = Path.Combine(Path.GetTempPath(), "receipt-exists-test-" + Guid.NewGuid().ToString("N") + ".pdf");
        try
        {
            // Create the file first
            File.WriteAllText(pdfPath, "existing");

            await Assert.ThrowsAsync<IOException>(() =>
                writer.WritePdfAsync(pdfPath, Dto(), settings, LabText(), "100"));
        }
        finally
        {
            if (File.Exists(pdfPath)) File.Delete(pdfPath);
        }
    }

    [Fact]
    public async Task WritePdfAsync_WithLogoAndPreprintedPaper_GeneratesPdf()
    {
        // Simulate pre-printed paper scenario: HeaderFooterMode.Images with logo
        var store = CreateStoreWithLogo(out var logoFileName);
        var settings = SettingsWithLogoAndFonts(logoFileName);
        var writer = new ReceiptPdfWriter(new BarcodeLabelRenderer(), store);

        var pdfPath = Path.Combine(Path.GetTempPath(), "receipt-preprinted-test-" + Guid.NewGuid().ToString("N") + ".pdf");
        try
        {
            await writer.WritePdfAsync(pdfPath, Dto(), settings, LabText(), "100");

            Assert.True(File.Exists(pdfPath));
            var bytes = File.ReadAllBytes(pdfPath);
            Assert.StartsWith("%PDF", Encoding.ASCII.GetString(bytes));
        }
        finally
        {
            if (File.Exists(pdfPath)) File.Delete(pdfPath);
        }
    }

    [Fact]
    public async Task WritePdfAsync_NullReceipt_Throws()
    {
        var settings = SettingsDefault();
        var writer = new ReceiptPdfWriter(new BarcodeLabelRenderer());
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            writer.WritePdfAsync("test.pdf", null!, settings, LabText(), "100"));
    }

    [Fact]
    public async Task WritePdfAsync_NullSettings_Throws()
    {
        var writer = new ReceiptPdfWriter(new BarcodeLabelRenderer());
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            writer.WritePdfAsync("test.pdf", Dto(), null!, LabText(), "100"));
    }

    [Fact]
    public async Task WritePdfAsync_NullLabText_Throws()
    {
        var settings = SettingsDefault();
        var writer = new ReceiptPdfWriter(new BarcodeLabelRenderer());
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            writer.WritePdfAsync("test.pdf", Dto(), settings, null!, "100"));
    }

    [Fact]
    public async Task WritePdfAsync_EmptyBarcodePayload_Throws()
    {
        var settings = SettingsDefault();
        var writer = new ReceiptPdfWriter(new BarcodeLabelRenderer());
        await Assert.ThrowsAsync<ArgumentException>(() =>
            writer.WritePdfAsync("test.pdf", Dto(), settings, LabText(), ""));
    }
}
