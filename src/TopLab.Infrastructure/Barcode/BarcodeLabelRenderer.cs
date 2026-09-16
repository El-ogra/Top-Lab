using ZXing;
using ZXing.Common;

namespace TopLab.Infrastructure.Barcode;

/// <summary>
/// Renders a Code-128 barcode bitmap for a data string (S-01 SD-1: ZXing.Net).
/// Pure byte/bitmap output, no WPF dependency. ZXing usage is confined here.
/// </summary>
public sealed class BarcodeLabelRenderer
{
    public BarcodeLabelData Render(string content, int width = 300, int height = 80)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = 2
            }
        };

        var pixelData = writer.Write(content);
        return new BarcodeLabelData(pixelData.Pixels, pixelData.Width, pixelData.Height);
    }
}

/// <summary>
/// Raw RGBA barcode bitmap (ZXing <c>PixelData</c> shape, decoupled for testability).
/// </summary>
public sealed record BarcodeLabelData(byte[] Pixels, int Width, int Height);
