using System.Text;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// P-01 F9 (PP-03) — <c>CustomGroupPdfWriter</c>.
///
/// Same two owner rules as S5, and both are enforced here:
///   1. The writer sets <c>QuestPDF.Settings.License</c> in its OWN static constructor.
///      Verified by running this file under <c>--filter</c> in isolation.
///   2. A PDF test must assert REAL output — file, size, %PDF header, %%EOF trailer,
///      and the expected content — not merely "it did not throw".
///
/// This file also asserts C-6 negatively: the group writer and the price-list writer are
/// **separate types** and neither DTO was widened to accommodate the other.
/// </summary>
public class CustomGroupPdfWriterTests
{
    private static LabPrintTextDto LabText() => new(
        LabName: "معمل النور للتحاليل",
        Address: "القاهرة",
        Phone: "0100000000",
        FontFamily: "Arial",
        FontSizePt: 11);

    private static CustomGroupDetailDto Group() => new(
        Id: 3,
        Name: "باقة الفحص الشامل",
        Items: new[]
        {
            new CustomGroupItemDto(201, "تحليل دهون", "LIPID", 300m),
            new CustomGroupItemDto(202, "إنزيمات كبد", "LFT", 275m),
            new CustomGroupItemDto(203, "سكر صائم", "FBS", 120m)
        });

    private static string TempPath(string tag) =>
        Path.Combine(Path.GetTempPath(), $"toplab-customgroup-{tag}-{Guid.NewGuid():N}.pdf");

    [Fact]
    public async Task CustomGroupPdfWriter_ProducesNonEmptyPdf()
    {
        var writer = new CustomGroupPdfWriter();
        var path = TempPath("nonempty");

        try
        {
            await writer.WritePdfAsync(path, Group(), LabText());

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
    public void CustomGroupPdfWriter_RendersGroupAndItems()
    {
        var group = Group();
        var lines = CustomGroupPdfWriter.BuildTextLines(group, LabText());

        Assert.Equal("معمل النور للتحاليل", Assert.Single(lines.Header));
        Assert.Equal("التحليل", lines.TestColumn);
        Assert.Equal("الكود", lines.CodeColumn);
        Assert.Equal("السعر", lines.PriceColumn);

        Assert.Equal(3, lines.Rows.Count);
        Assert.Contains(lines.Rows, r => r.TestName == "تحليل دهون" && r.TestCode == "LIPID" && r.Price == "300");
        Assert.Contains(lines.Rows, r => r.TestName == "إنزيمات كبد" && r.TestCode == "LFT" && r.Price == "275");
        Assert.Contains(lines.Rows, r => r.TestName == "سكر صائم" && r.TestCode == "FBS" && r.Price == "120");

        Assert.Equal("عدد التحاليل: 3", lines.TotalLine);
    }

    [Fact]
    public async Task CustomGroupPdfWriter_RenderedPdfIsLargerThanAnEmptyDocument()
    {
        var writer = new CustomGroupPdfWriter();
        var withItems = TempPath("with");
        var withoutItems = TempPath("without");
        var emptyGroup = new CustomGroupDetailDto(3, "باقة القلب", Array.Empty<CustomGroupItemDto>());

        try
        {
            await writer.WritePdfAsync(withItems, Group(), LabText());
            await writer.WritePdfAsync(withoutItems, emptyGroup, LabText());

            var withBytes = (await File.ReadAllBytesAsync(withItems)).Length;
            var withoutBytes = (await File.ReadAllBytesAsync(withoutItems)).Length;

            Assert.True(withBytes > withoutBytes,
                $"a 3-item group ({withBytes} bytes) must exceed an empty group ({withoutBytes} bytes)");
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
    public void CustomGroupPdfWriter_EmptyLabNameOmitsTheHeaderLine()
    {
        var noName = new LabPrintTextDto(LabName: string.Empty, Address: string.Empty, Phone: string.Empty, FontFamily: "Arial", FontSizePt: 11);
        var lines = CustomGroupPdfWriter.BuildTextLines(Group(), noName);

        Assert.Empty(lines.Header);
    }

    [Fact]
    public async Task CustomGroupPdfWriter_RejectsEmptyPath()
    {
        var writer = new CustomGroupPdfWriter();

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => writer.WritePdfAsync(string.Empty, Group(), LabText()));
        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => writer.WritePdfAsync("   ", Group(), LabText()));
    }

    [Fact]
    public async Task CustomGroupPdfWriter_RejectsNullArguments()
    {
        var writer = new CustomGroupPdfWriter();
        var path = TempPath("null");

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => writer.WritePdfAsync(path, null!, LabText()));
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => writer.WritePdfAsync(path, Group(), null!));

        Assert.Throws<ArgumentNullException>(
            () => CustomGroupPdfWriter.BuildTextLines(null!, LabText()));
        Assert.Throws<ArgumentNullException>(
            () => CustomGroupPdfWriter.BuildTextLines(Group(), null!));
    }

    [Fact]
    public async Task CustomGroupPdfWriter_NeverOverwritesAnExistingFile()
    {
        var writer = new CustomGroupPdfWriter();
        var path = TempPath("overwrite");
        await File.WriteAllBytesAsync(path, new byte[] { 1, 2, 3 });

        try
        {
            await Assert.ThrowsAsync<IOException>(
                () => writer.WritePdfAsync(path, Group(), LabText()));
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
    public async Task CustomGroupPdfWriter_RejectsAMissingDirectory()
    {
        var writer = new CustomGroupPdfWriter();
        var path = Path.Combine(Path.GetTempPath(), $"toplab-nodir-{Guid.NewGuid():N}", "x.pdf");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => writer.WritePdfAsync(path, Group(), LabText()));
    }

    // =====================================================================
    // C-6 — the two print paths must NOT share a writer or a DTO
    // =====================================================================

    [Fact]
    public void CustomGroupWriter_IsADistinctTypeFromThePriceListWriter()
    {
        Assert.NotEqual(typeof(CustomGroupPdfWriter), typeof(PriceListPdfWriter));
        Assert.False(typeof(CustomGroupPdfWriter).IsAssignableFrom(typeof(PriceListPdfWriter)));
        Assert.False(typeof(PriceListPdfWriter).IsAssignableFrom(typeof(CustomGroupPdfWriter)));
    }

    [Fact]
    public void CustomGroupWriter_DeclaresItsOwnTextLineTypes()
    {
        // Each writer owns its content-mapping records; neither borrows the other's.
        Assert.NotEqual(
            typeof(PriceListPdfWriter.PriceListTextLines),
            typeof(CustomGroupPdfWriter.CustomGroupTextLines));
        Assert.NotEqual(
            typeof(PriceListPdfWriter.PriceListTextRow),
            typeof(CustomGroupPdfWriter.CustomGroupTextRow));
    }

    [Fact]
    public void CustomGroupPort_IsADistinctTypeFromThePriceListPort()
    {
        var groupPort = typeof(TopLab.Application.Common.Interfaces.ICustomGroupPdfWriter);
        var pricePort = typeof(TopLab.Application.Common.Interfaces.IPriceListPdfWriter);

        Assert.NotEqual(groupPort, pricePort);
        Assert.False(groupPort.IsAssignableFrom(pricePort));
        Assert.False(pricePort.IsAssignableFrom(groupPort));

        // And each port takes only its own DTO.
        var groupParam = groupPort.GetMethod("WritePdfAsync")!.GetParameters()[1].ParameterType;
        var priceParam = pricePort.GetMethod("WritePdfAsync")!.GetParameters()[1].ParameterType;

        Assert.Equal(typeof(CustomGroupDetailDto), groupParam);
        Assert.Equal(typeof(PriceListDetailDto), priceParam);
        Assert.NotEqual(groupParam, priceParam);
    }
}