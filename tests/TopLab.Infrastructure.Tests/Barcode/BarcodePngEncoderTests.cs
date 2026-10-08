using TopLab.Infrastructure.Barcode;
using Xunit;

namespace TopLab.Infrastructure.Tests.Barcode;

public class BarcodePngEncoderTests
{
    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];

    private static int ReadBigEndian(byte[] bytes, int offset)
    {
        return (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
    }

    [Fact]
    public void Encode_NullPixels_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => BarcodePngEncoder.Encode(null!, 10, 10));
    }

    [Fact]
    public void Encode_NonPositiveDimensions_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BarcodePngEncoder.Encode(new byte[40], 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => BarcodePngEncoder.Encode(new byte[40], 10, 0));
    }

    [Fact]
    public void Encode_LengthMismatch_Throws()
    {
        Assert.Throws<ArgumentException>(() => BarcodePngEncoder.Encode(new byte[10], 2, 2));
    }

    [Fact]
    public void Encode_ValidRgba_ProducesPngWithMatchingDimensions()
    {
        var pixels = new byte[2 * 1 * 4];
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = 255;
        }

        var png = BarcodePngEncoder.Encode(pixels, 2, 1);

        Assert.Equal(PngSignature, png[..8]);
        Assert.Equal(2, ReadBigEndian(png, 16));
        Assert.Equal(1, ReadBigEndian(png, 20));
    }

    [Fact]
    public void Encode_SameInput_IsDeterministic()
    {
        var renderer = new BarcodeLabelRenderer();
        var label = renderer.Render("100");

        var first = BarcodePngEncoder.Encode(label.Pixels, label.Width, label.Height);
        var second = BarcodePngEncoder.Encode(label.Pixels, label.Width, label.Height);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Encode_RendererOutput_DistinctPayloadsGiveDistinctBytes()
    {
        var renderer = new BarcodeLabelRenderer();
        var first = renderer.Render("100");
        var second = renderer.Render("200");

        var firstPng = BarcodePngEncoder.Encode(first.Pixels, first.Width, first.Height);
        var secondPng = BarcodePngEncoder.Encode(second.Pixels, second.Width, second.Height);

        Assert.Equal(PngSignature, firstPng[..8]);
        Assert.Equal(PngSignature, secondPng[..8]);
        Assert.Equal(300, ReadBigEndian(firstPng, 16));
        Assert.Equal(80, ReadBigEndian(firstPng, 20));
        Assert.NotEqual(firstPng, secondPng);
    }
}
