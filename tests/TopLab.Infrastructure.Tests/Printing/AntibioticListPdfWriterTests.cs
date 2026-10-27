using System.Text;
using System.Text.RegularExpressions;
using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// GAP-25 (3.9.1.6.5) — <c>AntibioticListPdfWriter</c>.
///
/// The writer follows the price-list/custom-group conventions: its own static
/// constructor sets <c>QuestPDF.Settings.License</c>, and a PDF test asserts real
/// output (file exists, non-empty, %PDF header, %%EOF trailer), not merely that
/// no exception was thrown.
/// </summary>
public class AntibioticListPdfWriterTests
{
    private static LabPrintTextDto LabText() => new(
        LabName: "معمل النور للتحاليل",
        Address: "القاهرة",
        Phone: "0100000000",
        FontFamily: "Arial",
        FontSizePt: 11);

    private static IReadOnlyList<AntibioticDto> Catalogue() => new[]
    {
        new AntibioticDto(1, "Amoxicillin", true, false, "AMX", "Amoxicillin trihydrate", "Amoxil"),
        new AntibioticDto(2, "Ciprofloxacin", false, true, "CIP", "Ciprofloxacin hydrochloride", "Cipro"),
        new AntibioticDto(3, "Penicillin", false, false)
    };

    private static string TempPath(string tag) =>
        Path.Combine(Path.GetTempPath(), $"toplab-antibiotics-{tag}-{Guid.NewGuid():N}.pdf");

    // =====================================================================
    // Real output — not just "no throw"
    // =====================================================================

    [Fact]
    public async Task AntibioticListPdfWriter_ProducesNonEmptyPdf()
    {
        var writer = new AntibioticListPdfWriter();
        var path = TempPath("nonempty");

        try
        {
            await writer.WritePdfAsync(path, Catalogue(), LabText());

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
    public void AntibioticListPdfWriter_RendersNamesAndCommercialNames()
    {
        var lines = AntibioticListPdfWriter.BuildTextLines(Catalogue(), LabText());

        Assert.Equal("معمل النور للتحاليل", Assert.Single(lines.Header));
        Assert.Equal("قائمة المضادات الحيوية", lines.Title);
        Assert.Equal("المضاد", lines.NameColumn);
        Assert.Equal("الاسم العلمي", lines.ScientificNameColumn);
        Assert.Equal("الرمز", lines.SymbolColumn);
        Assert.Equal("الاسم التجاري", lines.CommercialNameColumn);

        // Order is the catalogue order supplied by the caller.
        Assert.Equal(new[] { "Amoxicillin", "Ciprofloxacin", "Penicillin" },
            lines.Rows.Select(r => r.Name));

        var amox = lines.Rows[0];
        Assert.Equal("Amoxicillin trihydrate", amox.ScientificName);
        Assert.Equal("AMX", amox.Symbol);
        Assert.Equal("Amoxil", amox.CommercialName);
        Assert.Equal("نعم", amox.PregnancyFlag);
        Assert.Equal("لا", amox.ChildrenFlag);

        var cipro = lines.Rows[1];
        Assert.Equal("Cipro", cipro.CommercialName);
        Assert.Equal("لا", cipro.PregnancyFlag);
        Assert.Equal("نعم", cipro.ChildrenFlag);

        Assert.Equal("عدد المضادات: 3", lines.TotalLine);
    }

    [Fact]
    public void AntibioticListPdfWriter_MissingOptionalValuesRenderAsEmptyCells()
    {
        var lines = AntibioticListPdfWriter.BuildTextLines(
            new[] { new AntibioticDto(3, "Penicillin", false, false) },
            LabText());

        var row = Assert.Single(lines.Rows);
        Assert.Equal("Penicillin", row.Name);
        Assert.Equal(string.Empty, row.ScientificName);
        Assert.Equal(string.Empty, row.Symbol);
        Assert.Equal(string.Empty, row.CommercialName);
    }

    [Fact]
    public async Task AntibioticListPdfWriter_EmptyCatalogueStillProducesAValidPdf()
    {
        var writer = new AntibioticListPdfWriter();
        var path = TempPath("empty");

        try
        {
            await writer.WritePdfAsync(path, Array.Empty<AntibioticDto>(), LabText());

            var bytes = await File.ReadAllBytesAsync(path);
            Assert.True(bytes.Length > 0);
            Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));

            var lines = AntibioticListPdfWriter.BuildTextLines(Array.Empty<AntibioticDto>(), LabText());
            Assert.Empty(lines.Rows);
            Assert.Equal("عدد المضادات: 0", lines.TotalLine);
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
    public async Task AntibioticListPdfWriter_RenderedPdfIsLargerThanAnEmptyCatalogue()
    {
        var writer = new AntibioticListPdfWriter();
        var withRows = TempPath("with");
        var withoutRows = TempPath("without");

        try
        {
            await writer.WritePdfAsync(withRows, Catalogue(), LabText());
            await writer.WritePdfAsync(withoutRows, Array.Empty<AntibioticDto>(), LabText());

            var withBytes = (await File.ReadAllBytesAsync(withRows)).Length;
            var withoutBytes = (await File.ReadAllBytesAsync(withoutRows)).Length;

            Assert.True(withBytes > withoutBytes,
                $"a 3-antibiotic list ({withBytes} bytes) must exceed an empty list ({withoutBytes} bytes)");
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

    [Fact]
    public async Task AntibioticListPdfWriter_LongCataloguePaginatesWithoutLosingContent()
    {
        var writer = new AntibioticListPdfWriter();
        var longPath = TempPath("long");
        var shortPath = TempPath("short");

        var longCatalogue = Enumerable.Range(1, 250)
            .Select(i => new AntibioticDto(
                i,
                $"Antibiotic {i:000}",
                i % 2 == 0,
                i % 3 == 0,
                $"S{i:000}",
                $"Scientific {i:000}",
                $"Commercial {i:000}"))
            .ToList();

        try
        {
            await writer.WritePdfAsync(longPath, longCatalogue, LabText());
            await writer.WritePdfAsync(shortPath, Catalogue(), LabText());

            var longBytes = await File.ReadAllBytesAsync(longPath);
            var shortBytes = await File.ReadAllBytesAsync(shortPath);

            Assert.Equal("%PDF", Encoding.ASCII.GetString(longBytes, 0, 4));
            Assert.True(longBytes.Length > shortBytes.Length,
                "a 250-row catalogue must produce a larger document than a 3-row one");

            var lines = AntibioticListPdfWriter.BuildTextLines(longCatalogue, LabText());
            Assert.Equal(250, lines.Rows.Count);
            Assert.Equal("Antibiotic 001", lines.Rows[0].Name);
            Assert.Equal("Commercial 250", lines.Rows[^1].CommercialName);
            Assert.Equal("عدد المضادات: 250", lines.TotalLine);
        }
        finally
        {
            if (File.Exists(longPath))
            {
                File.Delete(longPath);
            }

            if (File.Exists(shortPath))
            {
                File.Delete(shortPath);
            }
        }
    }

    /// <summary>
    /// GAP-25-AUDIT-01: the previous long-catalogue test proved only that a larger
    /// valid PDF was produced. This test proves the document is actually paginated:
    /// it reads the page tree written by QuestPDF (<c>/Type /Pages</c>,
    /// <c>/Count</c> and the <c>/Kids</c> array) and requires the three values to
    /// agree. A three-row catalogue must stay on one page; a 250-row catalogue must
    /// span more than one page and must not produce malformed page references.
    ///
    /// Per-page text extraction is not asserted: QuestPDF compresses the content
    /// streams with FlateDecode and uses subset fonts, and no PDF parser is
    /// referenced by this repository. Row completeness is covered separately by
    /// <see cref="AntibioticListPdfWriter_LongCataloguePaginatesWithoutLosingContent"/>.
    /// </summary>
    [Fact]
    public async Task AntibioticListPdfWriter_LongCatalogueProducesActualMultipleWellFormedPages()
    {
        var writer = new AntibioticListPdfWriter();
        var longPath = TempPath("pages-long");
        var shortPath = TempPath("pages-short");

        var longCatalogue = Enumerable.Range(1, 250)
            .Select(i => new AntibioticDto(
                i,
                $"Antibiotic {i:000}",
                i % 2 == 0,
                i % 3 == 0,
                $"S{i:000}",
                $"Scientific {i:000}",
                $"Commercial {i:000}"))
            .ToList();

        try
        {
            await writer.WritePdfAsync(longPath, longCatalogue, LabText());
            await writer.WritePdfAsync(shortPath, Catalogue(), LabText());

            var shortPdf = PdfPageStructure.Read(await File.ReadAllBytesAsync(shortPath));
            var longPdf = PdfPageStructure.Read(await File.ReadAllBytesAsync(longPath));

            Assert.Equal(1, shortPdf.PageObjects);
            Assert.Equal(1, shortPdf.DeclaredPageCount);
            Assert.Equal(1, shortPdf.KidReferences);

            Assert.True(longPdf.PageObjects > 1,
                $"a 250-row catalogue must produce more than one page object; found {longPdf.PageObjects}");
            Assert.Equal(longPdf.PageObjects, longPdf.DeclaredPageCount);
            Assert.Equal(longPdf.PageObjects, longPdf.KidReferences);
            Assert.True(longPdf.MediaBoxes > 0, "the paginated pages must carry page geometry");
        }
        finally
        {
            if (File.Exists(longPath))
            {
                File.Delete(longPath);
            }

            if (File.Exists(shortPath))
            {
                File.Delete(shortPath);
            }
        }
    }

    /// <summary>
    /// Minimal, dependency-free structural read of the PDF page tree. QuestPDF
    /// 2026.9.0 writes uncompressed object dictionaries (no <c>/ObjStm</c>), so the
    /// page tree is visible in the file bytes; this is asserted by requiring the
    /// <c>/Type /Pages</c> node to exist.
    /// </summary>
    private sealed record PdfPageStructure(
        int PageObjects,
        int DeclaredPageCount,
        int KidReferences,
        int MediaBoxes)
    {
        public static PdfPageStructure Read(byte[] bytes)
        {
            var text = Encoding.Latin1.GetString(bytes);
            Assert.Contains("/Type /Pages", text, StringComparison.Ordinal);

            var pageObjects = Regex.Matches(text, @"/Type\s*/Page(?![s])").Count;

            var pagesNode = Regex.Match(text, @"/Type\s*/Pages\s*/Count\s+(\d+)\s*/Kids\s*\[([^\]]*)\]");
            Assert.True(pagesNode.Success, "the PDF must contain a well-formed page tree with /Count and /Kids");

            var declaredPageCount = int.Parse(pagesNode.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
            var kidReferences = Regex.Matches(pagesNode.Groups[2].Value, @"\d+\s+0\s+R").Count;
            var mediaBoxes = Regex.Matches(text, @"/MediaBox").Count;

            return new PdfPageStructure(pageObjects, declaredPageCount, kidReferences, mediaBoxes);
        }
    }

    [Fact]
    public void AntibioticListPdfWriter_EmptyLabNameOmitsTheHeaderLine()
    {
        var noName = new LabPrintTextDto(
            LabName: string.Empty,
            Address: string.Empty,
            Phone: string.Empty,
            FontFamily: "Arial",
            FontSizePt: 11);

        var lines = AntibioticListPdfWriter.BuildTextLines(Catalogue(), noName);

        Assert.Empty(lines.Header);
    }

    // =====================================================================
    // Argument validation and output safety
    // =====================================================================

    [Fact]
    public async Task AntibioticListPdfWriter_RejectsEmptyPath()
    {
        var writer = new AntibioticListPdfWriter();

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => writer.WritePdfAsync(string.Empty, Catalogue(), LabText()));
        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => writer.WritePdfAsync("   ", Catalogue(), LabText()));
    }

    [Fact]
    public async Task AntibioticListPdfWriter_RejectsNullArguments()
    {
        var writer = new AntibioticListPdfWriter();
        var path = TempPath("null");

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => writer.WritePdfAsync(path, null!, LabText()));
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => writer.WritePdfAsync(path, Catalogue(), null!));

        Assert.Throws<ArgumentNullException>(
            () => AntibioticListPdfWriter.BuildTextLines(null!, LabText()));
        Assert.Throws<ArgumentNullException>(
            () => AntibioticListPdfWriter.BuildTextLines(Catalogue(), null!));
    }

    [Fact]
    public async Task AntibioticListPdfWriter_NeverOverwritesAnExistingFile()
    {
        var writer = new AntibioticListPdfWriter();
        var path = TempPath("overwrite");
        await File.WriteAllBytesAsync(path, new byte[] { 1, 2, 3 });

        try
        {
            await Assert.ThrowsAsync<IOException>(
                () => writer.WritePdfAsync(path, Catalogue(), LabText()));
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
    public async Task AntibioticListPdfWriter_RejectsAMissingDirectory()
    {
        var writer = new AntibioticListPdfWriter();
        var path = Path.Combine(Path.GetTempPath(), $"toplab-nodir-{Guid.NewGuid():N}", "x.pdf");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => writer.WritePdfAsync(path, Catalogue(), LabText()));
    }
}
