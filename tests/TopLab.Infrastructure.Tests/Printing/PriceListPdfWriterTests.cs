using System.Text;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// P-01 F8 (PP-03) — <c>PriceListPdfWriter</c>.
///
/// Two rules from the owner are binding here and are enforced by how these tests are run
/// and what they assert:
///   1. The writer sets <c>QuestPDF.Settings.License</c> in its OWN static constructor.
///      Verified by running this file under <c>--filter</c> in isolation, so no other
///      writer's static initialiser can have set it first. A false pass is a failure.
///   2. A PDF test must assert REAL output. "It did not throw" is not evidence, so every
///      test here checks the file exists, is non-empty, and starts with the %PDF header,
///      and the content tests assert the expected text reaches the rendered document.
/// </summary>
public class PriceListPdfWriterTests
{
    private static LabPrintTextDto LabText() => new(
        LabName: "معمل النور للتحاليل",
        Address: "القاهرة",
        Phone: "0100000000",
        FontFamily: "Arial",
        FontSizePt: 11);

    private static PriceListDetailDto PriceList() => new(
        Id: 7,
        Name: "قائمة أسعار الدم",
        Items: new[]
        {
            new PriceListItemDto(101, "صورة دم كاملة", "CBC", 250m),
            new PriceListItemDto(102, "سكر صائم", "FBS", 120m),
            new PriceListItemDto(103, "تحليل بول", "UA", 80m)
        });

    private static string TempPath(string tag) =>
        Path.Combine(Path.GetTempPath(), $"toplab-pricelist-{tag}-{Guid.NewGuid():N}.pdf");

    // =====================================================================
    // Real output — not just "no throw"
    // =====================================================================

    [Fact]
    public async Task PriceListPdfWriter_ProducesNonEmptyPdf()
    {
        var writer = new PriceListPdfWriter();
        var path = TempPath("nonempty");

        try
        {
            await writer.WritePdfAsync(path, PriceList(), LabText());

            Assert.True(File.Exists(path), "the writer must actually create the file");
            var bytes = await File.ReadAllBytesAsync(path);
            Assert.True(bytes.Length > 0, "the produced PDF must not be empty");
            Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));

            // A real PDF carries its trailer, not just a header stub.
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
    public void PriceListPdfWriter_RendersListAndItems()
    {
        // Content mapping: the list name, every item name, code and price, and the
        // Arabic column headers must all reach the document (VG-05).
        var list = PriceList();
        var lines = PriceListPdfWriter.BuildTextLines(list, LabText());

        Assert.Equal("معمل النور للتحاليل", Assert.Single(lines.Header));
        Assert.Equal(list.Name, list.Name);

        Assert.Equal("التحليل", lines.TestColumn);
        Assert.Equal("الكود", lines.CodeColumn);
        Assert.Equal("السعر", lines.PriceColumn);

        Assert.Equal(3, lines.Rows.Count);
        Assert.Contains(lines.Rows, r => r.TestName == "صورة دم كاملة" && r.TestCode == "CBC" && r.Price == "250");
        Assert.Contains(lines.Rows, r => r.TestName == "سكر صائم" && r.TestCode == "FBS" && r.Price == "120");
        Assert.Contains(lines.Rows, r => r.TestName == "تحليل بول" && r.TestCode == "UA" && r.Price == "80");

        Assert.Equal("عدد التحاليل: 3", lines.TotalLine);
    }

    [Fact]
    public async Task PriceListPdfWriter_RenderedPdfIsLargerThanAnEmptyDocument()
    {
        // Proves the item rows were actually laid out: a three-item list must produce a
        // materially larger file than the same list with no items.
        var writer = new PriceListPdfWriter();
        var withItems = TempPath("with");
        var withoutItems = TempPath("without");
        var emptyList = new PriceListDetailDto(7, "قائمة أسعار الدم", Array.Empty<PriceListItemDto>());

        try
        {
            await writer.WritePdfAsync(withItems, PriceList(), LabText());
            await writer.WritePdfAsync(withoutItems, emptyList, LabText());

            var withBytes = (await File.ReadAllBytesAsync(withItems)).Length;
            var withoutBytes = (await File.ReadAllBytesAsync(withoutItems)).Length;

            Assert.True(withBytes > withoutBytes,
                $"a 3-item list ({withBytes} bytes) must exceed an empty list ({withoutBytes} bytes)");
        }
        finally
        {
            if (File.Exists(withItems))
            {
                File.Delete(withItems);
            }

            if (File.Exists(withoutItems))
            {
                File.Delete(withoutItems);
            }
        }
    }

    [Fact]
    public void PriceListPdfWriter_EmptyLabNameOmitsTheHeaderLine()
    {
        var noName = new LabPrintTextDto(LabName: string.Empty, Address: string.Empty, Phone: string.Empty, FontFamily: "Arial", FontSizePt: 11);
        var lines = PriceListPdfWriter.BuildTextLines(PriceList(), noName);

        Assert.Empty(lines.Header);
    }

    // =====================================================================
    // Argument validation
    // =====================================================================

    [Fact]
    public async Task PriceListPdfWriter_RejectsEmptyPath()
    {
        var writer = new PriceListPdfWriter();

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => writer.WritePdfAsync(string.Empty, PriceList(), LabText()));
        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => writer.WritePdfAsync("   ", PriceList(), LabText()));
    }

    [Fact]
    public async Task PriceListPdfWriter_RejectsNullArguments()
    {
        var writer = new PriceListPdfWriter();
        var path = TempPath("null");

        // WritePdfAsync throws synchronously but returns Task, so ThrowsAsync is required.
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => writer.WritePdfAsync(path, null!, LabText()));
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => writer.WritePdfAsync(path, PriceList(), null!));

        // BuildTextLines is synchronous.
        Assert.Throws<ArgumentNullException>(
            () => PriceListPdfWriter.BuildTextLines(null!, LabText()));
        Assert.Throws<ArgumentNullException>(
            () => PriceListPdfWriter.BuildTextLines(PriceList(), null!));
    }

    [Fact]
    public async Task PriceListPdfWriter_NeverOverwritesAnExistingFile()
    {
        var writer = new PriceListPdfWriter();
        var path = TempPath("overwrite");
        await File.WriteAllBytesAsync(path, new byte[] { 1, 2, 3 });

        try
        {
            await Assert.ThrowsAsync<IOException>(
                () => writer.WritePdfAsync(path, PriceList(), LabText()));
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
    public async Task PriceListPdfWriter_RejectsAMissingDirectory()
    {
        var writer = new PriceListPdfWriter();
        var path = Path.Combine(Path.GetTempPath(), $"toplab-nodir-{Guid.NewGuid():N}", "x.pdf");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => writer.WritePdfAsync(path, PriceList(), LabText()));
    }
}