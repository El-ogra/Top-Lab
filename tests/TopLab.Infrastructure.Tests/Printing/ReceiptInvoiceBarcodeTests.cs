using System.Text;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Barcode;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// Phase 1 REF-127: scannable identifier barcodes on the receipt and invoice.
/// Proves the payload matrix at both writers' mapping level and proves both
/// generated PDFs embed an image XObject (not text-only), through the shared
/// REF-066 pipeline. The envelope half is the REF-067 suite re-run green.
/// </summary>
public class ReceiptInvoiceBarcodeTests
{
    private static ReceiptDto ReceiptDto(string? labId = "100")
    {
        return new ReceiptDto(
            7,
            "أحمد محمد علي",
            labId,
            new List<ChargedTestDto>
            {
                new(1, "صورة دم كاملة", "CBC", "CBC", 100m)
            },
            100m,
            10m,
            40m,
            50m,
            "L.E.");
    }

    private static InvoiceDto InvoiceDto(string? labId = "100")
    {
        return new InvoiceDto(
            7,
            "أحمد محمد علي",
            labId,
            42,
            new DateTime(2026, 9, 16, 10, 0, 0, DateTimeKind.Utc),
            new List<ChargedTestDto>
            {
                new(1, "صورة دم كاملة", "CBC", "CBC", 100m)
            },
            100m,
            10m,
            40m,
            50m,
            "L.E.");
    }

    private static ReceiptSettings CashierSettings()
    {
        var settings = ReceiptSettings.CreateDefault();
        settings.Update(1m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.Words);
        return settings;
    }

    private static LabPrintTextDto LabText()
    {
        return new LabPrintTextDto("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12);
    }

    [Theory]
    [InlineData("100", true, "100")]
    [InlineData("100", false, "7")]
    [InlineData(null, true, "7")]
    [InlineData("", true, "7")]
    public void ReceiptBuildTextLines_BarcodePayload_FollowsIdentifierRule(
        string? labId, bool printLabIdInstead, string expected)
    {
        var payload = BarcodePayload.For(7, labId, printLabIdInstead);

        var lines = ReceiptPdfWriter.BuildTextLines(ReceiptDto(labId), CashierSettings(), LabText(), payload);

        Assert.Equal(expected, payload);
        Assert.Equal(expected, lines.BarcodePayload);
    }

    [Theory]
    [InlineData("100", true, "100")]
    [InlineData("100", false, "7")]
    [InlineData(null, true, "7")]
    [InlineData("", true, "7")]
    public void InvoiceBuildTextLines_BarcodePayload_FollowsIdentifierRule(
        string? labId, bool printLabIdInstead, string expected)
    {
        var payload = BarcodePayload.For(7, labId, printLabIdInstead);

        var lines = InvoicePdfWriter.BuildTextLines(InvoiceDto(labId), LabText(), payload);

        Assert.Equal(expected, payload);
        Assert.Equal(expected, lines.BarcodePayload);
    }

    [Fact]
    public async Task ReceiptPdf_EmbedsBarcodeImageObject()
    {
        var writer = new ReceiptPdfWriter(new BarcodeLabelRenderer());
        var path = Path.Combine(Path.GetTempPath(), $"TopLabTest-Receipt-{Guid.NewGuid():N}.pdf");
        try
        {
            await writer.WritePdfAsync(path, ReceiptDto(), CashierSettings(), LabText(), "100");

            var content = Encoding.ASCII.GetString(File.ReadAllBytes(path));
            Assert.Contains("/Subtype /Image", content, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task InvoicePdf_EmbedsBarcodeImageObject()
    {
        var writer = new InvoicePdfWriter(new BarcodeLabelRenderer());
        var path = Path.Combine(Path.GetTempPath(), $"TopLabTest-Invoice-{Guid.NewGuid():N}.pdf");
        try
        {
            await writer.WritePdfAsync(path, InvoiceDto(), LabText(), "100");

            var content = Encoding.ASCII.GetString(File.ReadAllBytes(path));
            Assert.Contains("/Subtype /Image", content, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
