using System.Text;
using System.Text.Json;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// GAP-011 (Phase 3): the persisted header/footer images must render on the report
/// for both paper choices, with a words fallback when the image file is missing, and
/// must never collide with pre-printed stationery or abort PDF generation.
/// </summary>
public class ReportPdfWriterLogoRenderingTests : IDisposable
{
    private readonly string _dir;

    public ReportPdfWriterLogoRenderingTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "toplab-report-logo-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        File.WriteAllBytes(Path.Combine(_dir, "Header.png"), TestPng.Create(1600, 200));
        File.WriteAllBytes(Path.Combine(_dir, "Footer.png"), TestPng.Create(1600, 120));
        // Deliberately far taller than the page: the pre-printed-stationery collision case.
        File.WriteAllBytes(Path.Combine(_dir, "Tall.png"), TestPng.Create(200, 4000));
    }

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    private ReportPdfWriter Writer() =>
        new(nameResolver: null, labTextStore: null, imageAssetStore: new TempAssetStore(_dir));

    private static ReportPrintEnvelope CombinedEnvelope()
    {
        var dto = new CombinedReportDto(7, "علي", "LAB-1", new List<CombinedReportLineDto>
        {
            new(101, 2, "سكر الدم", "GLU", 0, "5.5", 0, "4 - 6", new List<ProfileReportLineDto>(), null)
        });
        return new ReportPrintEnvelope(ReportPrintEnvelope.Combined, JsonSerializer.Serialize(dto));
    }

    private async Task<string> WriteAsync(
        HeaderFooterMode mode,
        string? headerImage,
        string? footerImage,
        PaperSize paperSize = PaperSize.A4)
    {
        var settings = ReportSettings.CreateDefault();
        settings.SetPaperSize(paperSize);
        settings.SetHeaderFooterMode(mode);
        settings.SetHeaderImagePath(headerImage);
        settings.SetFooterImagePath(footerImage);

        var path = Path.Combine(_dir, $"out-{Guid.NewGuid():N}.pdf");
        await Writer().WritePdfAsync(path, CombinedEnvelope(), settings, SystemSettings.CreateDefault());
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

    // ---- acceptance criterion 1: plain paper (the default path) ----

    [Fact]
    public async Task PlainPaper_HeaderImage_Renders()
    {
        var path = await WriteAsync(HeaderFooterMode.Images, "Header.png", null);
        Assert.Equal("%PDF", Latin(path)[..4]);
        Assert.Equal(1, ImageObjects(path));
    }

    [Fact]
    public async Task PlainPaper_FooterImage_Renders()
    {
        var path = await WriteAsync(HeaderFooterMode.Images, null, "Footer.png");
        Assert.Equal(1, ImageObjects(path));
    }

    [Fact]
    public async Task PlainPaper_HeaderAndFooterImages_BothRender()
    {
        var path = await WriteAsync(HeaderFooterMode.Images, "Header.png", "Footer.png");
        Assert.Equal(2, ImageObjects(path));
    }

    // ---- acceptance criterion 2: the other paper choice (A5) ----

    [Fact]
    public async Task A5Paper_HeaderImage_Renders()
    {
        var path = await WriteAsync(HeaderFooterMode.Images, "Header.png", null, PaperSize.A5);
        Assert.Equal(1, ImageObjects(path));
        // A5 portrait = 420 x 595 pt
        Assert.Contains("420", Latin(path));
        Assert.Contains("595", Latin(path));
    }

    [Fact]
    public async Task A5Paper_HeaderAndFooterImages_BothRender()
    {
        var path = await WriteAsync(HeaderFooterMode.Images, "Header.png", "Footer.png", PaperSize.A5);
        Assert.Equal(2, ImageObjects(path));
    }

    // ---- validation rule: fall back to words when the file is missing ----

    [Fact]
    public async Task MissingImageFile_FallsBackToWordsHeader()
    {
        var path = await WriteAsync(HeaderFooterMode.Images, "DoesNotExist.png", null);
        Assert.Equal(0, ImageObjects(path));

        var settings = ReportSettings.CreateDefault();
        settings.SetHeaderFooterMode(HeaderFooterMode.Images);
        settings.SetHeaderImagePath("DoesNotExist.png");
        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(), settings, SystemSettings.CreateDefault(),
            labName: "مختبر الشفاء",
            resolvedHeaderImagePath: null);
        Assert.Contains(content.BuildDisplayLines(), l => l.Contains("مختبر الشفاء", StringComparison.Ordinal));
    }

    [Fact]
    public async Task NoImageConfigured_RendersNoImage()
    {
        var path = await WriteAsync(HeaderFooterMode.Images, null, null);
        Assert.Equal(0, ImageObjects(path));
    }

    [Fact]
    public async Task HeaderFooterModeNone_RendersNoImageEvenWhenConfigured()
    {
        var path = await WriteAsync(HeaderFooterMode.None, "Header.png", "Footer.png");
        Assert.Equal(0, ImageObjects(path));
    }

    // ---- risk mitigation: the logo must not collide with pre-printed stationery ----

    [Fact]
    public async Task OverTallHeaderImage_DoesNotAbortPdfGeneration()
    {
        // Before the bound, an over-tall logo exhausted the page and QuestPDF threw
        // DocumentLayoutException, so no report could be produced at all.
        var path = await WriteAsync(HeaderFooterMode.Images, "Tall.png", null);
        Assert.Equal("%PDF", Latin(path)[..4]);
        Assert.Equal(1, ImageObjects(path));
    }

    [Fact]
    public async Task OverTallFooterImage_DoesNotAbortPdfGeneration()
    {
        var path = await WriteAsync(HeaderFooterMode.Images, null, "Tall.png");
        Assert.Equal("%PDF", Latin(path)[..4]);
        Assert.Equal(1, ImageObjects(path));
    }

    [Fact]
    public void FitHeaderFooterImage_CapsHeightAndUsesContentWidth()
    {
        // A4 = 595 pt wide, 1 cm margins -> 595 - 2*28.3465 content width.
        var (width, maxHeight) = PageSizeMapper.FitHeaderFooterImage(PaperSize.A4, 1m, 200f);
        Assert.Equal(595f - (2f * 28.3465f), width, 2);
        Assert.Equal(200f, maxHeight, 2);

        // A5 is narrower, so the content width shrinks with it.
        var (a5Width, _) = PageSizeMapper.FitHeaderFooterImage(PaperSize.A5, 1m, 200f);
        Assert.Equal(420f - (2f * 28.3465f), a5Width, 2);
    }

    [Fact]
    public void FitHeaderFooterImage_DegenerateGeometry_NeverNegative()
    {
        var (width, maxHeight) = PageSizeMapper.FitHeaderFooterImage(PaperSize.A5, 30m, -5f);
        Assert.Equal(0f, width);
        Assert.Equal(0f, maxHeight);
    }

    [Fact]
    public void ImageHeightCap_IsAFractionOfThePrintableContentHeight()
    {
        var fraction = PageSizeMapper.HeaderFooterImageMaxHeightFraction;
        Assert.True(fraction > 0f && fraction < 1f);

        // Worst case: no top space and no bottom margin still leaves the body room.
        var (_, a4Height) = PageSizeMapper.ToPoints(PaperSize.A4);
        Assert.True(a4Height * fraction < a4Height);
    }

    /// <summary>Test double: resolves images from a throwaway directory, never %ProgramData%.</summary>
    private sealed class TempAssetStore : IReportImageAssetStore
    {
        private readonly string _directory;

        public TempAssetStore(string directory) => _directory = directory;

        public string? StoreImage(string sourcePath, ReportImageTarget target) => null;

        public string? ResolveImagePath(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            var full = Path.Combine(_directory, relativePath);
            return File.Exists(full) ? full : null;
        }

        public string GetAssetDirectory() => _directory;
    }
}

/// <summary>
/// Minimal PNG encoder for tests: writes a real image of an exact pixel size so
/// aspect-ratio and over-tall behaviour can be exercised (a 1x1 stub cannot).
/// </summary>
internal static class TestPng
{
    public static byte[] Create(int width, int height)
    {
        var raw = new byte[height * (1 + width * 3)];
        for (var y = 0; y < height; y++)
        {
            var row = y * (1 + width * 3);
            for (var x = 0; x < width; x++)
            {
                raw[row + 1 + (x * 3)] = 0;
                raw[row + 2 + (x * 3)] = 0;
                raw[row + 3 + (x * 3)] = 255;
            }
        }

        using var stream = new MemoryStream();
        stream.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

        var ihdr = new byte[13];
        WriteBigEndian(ihdr, 0, width);
        WriteBigEndian(ihdr, 4, height);
        ihdr[8] = 8;  // bit depth
        ihdr[9] = 2;  // truecolour RGB
        WriteChunk(stream, "IHDR", ihdr);
        WriteChunk(stream, "IDAT", ZlibStored(raw));
        WriteChunk(stream, "IEND", Array.Empty<byte>());
        return stream.ToArray();
    }

    private static void WriteBigEndian(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        var length = BitConverter.GetBytes(data.Length);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(length);
        }

        stream.Write(length);
        var typeBytes = Encoding.ASCII.GetBytes(type);
        stream.Write(typeBytes);
        stream.Write(data);

        var crc = BitConverter.GetBytes(Crc32(typeBytes.Concat(data).ToArray()));
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(crc);
        }

        stream.Write(crc);
    }

    /// <summary>zlib stream using stored (uncompressed) deflate blocks — no compressor dependency.</summary>
    private static byte[] ZlibStored(byte[] data)
    {
        using var stream = new MemoryStream();
        stream.WriteByte(0x78);
        stream.WriteByte(0x01);

        var position = 0;
        while (position < data.Length)
        {
            var size = Math.Min(65535, data.Length - position);
            var isLast = position + size >= data.Length;
            stream.WriteByte((byte)(isLast ? 1 : 0));
            stream.WriteByte((byte)(size & 0xFF));
            stream.WriteByte((byte)((size >> 8) & 0xFF));
            stream.WriteByte((byte)(~size & 0xFF));
            stream.WriteByte((byte)((~size >> 8) & 0xFF));
            stream.Write(data, position, size);
            position += size;
        }

        uint a = 1, b = 0;
        foreach (var value in data)
        {
            a = (a + value) % 65521;
            b = (b + a) % 65521;
        }

        var adler = (b << 16) | a;
        stream.WriteByte((byte)(adler >> 24));
        stream.WriteByte((byte)(adler >> 16));
        stream.WriteByte((byte)(adler >> 8));
        stream.WriteByte((byte)adler);
        return stream.ToArray();
    }

    private static readonly uint[] Table = BuildTable();

    private static uint[] BuildTable()
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            var c = i;
            for (var k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
            }

            table[i] = c;
        }

        return table;
    }

    private static uint Crc32(byte[] data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var value in data)
        {
            crc = Table[(crc ^ value) & 0xFF] ^ (crc >> 8);
        }

        return crc ^ 0xFFFFFFFFu;
    }
}
