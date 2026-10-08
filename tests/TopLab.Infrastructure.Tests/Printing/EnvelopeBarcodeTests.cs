using System.IO.Compression;
using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Barcode;
using TopLab.Infrastructure.Printing;
using ZXing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// Phase 1 REF-067: the envelope Code-item barcode.
/// The writer branch itself (image + readable text + silent text fallback) was
/// wired in REF-066; these tests prove the acceptance behavior: the PNG bytes
/// decode to exactly the identifier-rule payload, the payload matrix holds at
/// the writer-mapping level, no datetime suffix ever leaks in, and a disabled
/// Code item leaves no barcode behind.
/// </summary>
public class EnvelopeBarcodeTests
{
    private static EnvelopeDto Dto(string identifier, string? referral = "د. أحمد")
    {
        return new EnvelopeDto(
            7,
            "أحمد محمد علي",
            identifier,
            referral,
            new DateTime(2026, 10, 1, 9, 30, 0, DateTimeKind.Utc),
            "100");
    }

    private static EnvelopeSettings Settings()
    {
        var settings = EnvelopeSettings.CreateDefault();
        settings.Update(3.0m, HeaderFooterMode.Words, false);
        return settings;
    }

    private static List<EnvelopePrintItemPosition> Positions(bool codeEnabled = true)
    {
        return new List<EnvelopePrintItemPosition>
        {
            new("Name", true, 1.0m, 1.0m),
            new("Code", codeEnabled, 1.0m, 2.0m),
            new("ReferralEntity", true, 1.0m, 3.0m),
            new("Date", true, 1.0m, 4.0m)
        };
    }

    private static LabPrintTextDto LabText()
    {
        return new LabPrintTextDto("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12);
    }

    [Theory]
    [InlineData(7, "100", true, "100")]
    [InlineData(7, "100", false, "7")]
    [InlineData(7, null, true, "7")]
    [InlineData(9, "", true, "9")]
    public void BuildTextLines_CodePayload_FollowsIdentifierRule(
        int patientId, string? labId, bool printLabIdInstead, string expected)
    {
        var identifier = BarcodePayload.For(patientId, labId, printLabIdInstead);

        var lines = EnvelopePdfWriter.BuildTextLines(Dto(identifier), Settings(), Positions(), LabText());

        Assert.Equal(expected, identifier);
        Assert.Equal(expected, lines.CodeBarcodePayload);
        Assert.Contains(expected, lines.Items.Single(i => i.ItemName == "Code").Text);
    }

    [Fact]
    public void BuildTextLines_CodePayload_IsExactlyTheIdentifier_NoDatetimeSuffix()
    {
        var lines = EnvelopePdfWriter.BuildTextLines(Dto("100"), Settings(), Positions(), LabText());

        Assert.Equal("100", lines.CodeBarcodePayload);
        Assert.DoesNotContain(" ", lines.CodeBarcodePayload);
    }

    [Fact]
    public void EnvelopeBarcodePng_DecodesToExactlyTheIdentifier()
    {
        var renderer = new BarcodeLabelRenderer();
        var label = renderer.Render("100");
        var png = BarcodePngEncoder.Encode(label.Pixels, label.Width, label.Height);

        var rgb = DecodePngToRgb24(png, out var width, out var height);

        var reader = new BarcodeReaderGeneric();
        var decoded = reader.Decode(rgb, width, height, RGBLuminanceSource.BitmapFormat.RGB24);

        Assert.NotNull(decoded);
        Assert.Equal("100", decoded.Text);
    }

    [Fact]
    public void EnvelopeBarcodePng_DecodesLabIdPayload()
    {
        var renderer = new BarcodeLabelRenderer();
        var identifier = BarcodePayload.For(7, "LAB-77", printLabIdInstead: true);
        var label = renderer.Render(identifier);
        var png = BarcodePngEncoder.Encode(label.Pixels, label.Width, label.Height);

        var rgb = DecodePngToRgb24(png, out var width, out var height);

        var decoded = new BarcodeReaderGeneric().Decode(rgb, width, height, RGBLuminanceSource.BitmapFormat.RGB24);

        Assert.NotNull(decoded);
        Assert.Equal("LAB-77", decoded.Text);
    }

    [Fact]
    public void BuildTextLines_CodeDisabled_HasNoBarcodePayload()
    {
        var lines = EnvelopePdfWriter.BuildTextLines(Dto("100"), Settings(), Positions(codeEnabled: false), LabText());

        Assert.Null(lines.CodeBarcodePayload);
        Assert.DoesNotContain(lines.Items, i => i.ItemName == "Code");
    }

    /// <summary>
    /// Minimal PNG decoder for test verification only: parses our encoder's
    /// output (8-bit truecolor RGB, filter type 0) back to raw RGB24.
    /// </summary>
    private static byte[] DecodePngToRgb24(byte[] png, out int width, out int height)
    {
        var signature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        Assert.Equal(signature, png[..8]);

        width = 0;
        height = 0;
        var idat = new List<byte>();
        var offset = 8;
        while (offset < png.Length)
        {
            var length = (png[offset] << 24) | (png[offset + 1] << 16) | (png[offset + 2] << 8) | png[offset + 3];
            var type = System.Text.Encoding.ASCII.GetString(png, offset + 4, 4);
            var data = png[(offset + 8)..(offset + 8 + length)];
            if (type == "IHDR")
            {
                width = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
                height = (data[4] << 24) | (data[5] << 16) | (data[6] << 8) | data[7];
                Assert.Equal(8, data[8]);
                Assert.Equal(2, data[9]);
            }
            else if (type == "IDAT")
            {
                idat.AddRange(data);
            }
            else if (type == "IEND")
            {
                break;
            }

            offset += 8 + length + 4;
        }

        Assert.True(width > 0 && height > 0);

        byte[] raw;
        using (var input = new MemoryStream(idat.ToArray(), 2, idat.Count - 6))
        using (var inflater = new DeflateStream(input, CompressionMode.Decompress))
        using (var output = new MemoryStream())
        {
            inflater.CopyTo(output);
            raw = output.ToArray();
        }

        var stride = 1 + width * 3;
        Assert.Equal(height * stride, raw.Length);

        var rgb = new byte[width * height * 3];
        for (var y = 0; y < height; y++)
        {
            Assert.Equal(0, raw[y * stride]);
            Buffer.BlockCopy(raw, y * stride + 1, rgb, y * width * 3, width * 3);
        }

        return rgb;
    }
}
