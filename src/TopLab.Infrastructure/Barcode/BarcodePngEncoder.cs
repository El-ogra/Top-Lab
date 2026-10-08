using System.IO.Compression;

namespace TopLab.Infrastructure.Barcode;

/// <summary>
/// Minimal deterministic RGBA → PNG encoder (Phase 1, REF-066 — shared owner;
/// consumed by REF-066/067/068/127).
/// Bridges <c>BarcodeLabelRenderer</c> (ZXing RGBA pixels) to QuestPDF
/// <c>.Image(byte[])</c> with zero new NuGet packages (decision 66-A).
/// Truecolor RGB, one IDAT, no interlace. Throws on invalid input — writers and
/// services map failures to <c>Error.Unexpected</c>.
/// </summary>
public static class BarcodePngEncoder
{
    private static readonly uint[] CrcTable = BuildCrcTable();

    public static byte[] Encode(byte[] rgbaPixels, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(rgbaPixels);
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        if (rgbaPixels.Length != width * height * 4)
        {
            throw new ArgumentException("Pixel buffer length must equal width * height * 4 (RGBA).", nameof(rgbaPixels));
        }

        var raw = new byte[height * (1 + width * 3)];
        for (var y = 0; y < height; y++)
        {
            raw[y * (1 + width * 3)] = 0; // filter type 0 (None)
            for (var x = 0; x < width; x++)
            {
                var src = (y * width + x) * 4;
                var dst = y * (1 + width * 3) + 1 + x * 3;
                raw[dst] = rgbaPixels[src];
                raw[dst + 1] = rgbaPixels[src + 1];
                raw[dst + 2] = rgbaPixels[src + 2];
            }
        }

        using var output = new MemoryStream();
        output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, 0, 8);

        var ihdr = new byte[13];
        WriteBigEndian(ihdr, 0, (uint)width);
        WriteBigEndian(ihdr, 4, (uint)height);
        ihdr[8] = 8; // bit depth
        ihdr[9] = 2; // color type: truecolor RGB
        ihdr[10] = 0; // compression
        ihdr[11] = 0; // filter
        ihdr[12] = 0; // interlace
        WriteChunk(output, "IHDR", ihdr);
        WriteChunk(output, "IDAT", ZlibCompress(raw));
        WriteChunk(output, "IEND", Array.Empty<byte>());

        return output.ToArray();
    }

    private static byte[] ZlibCompress(byte[] raw)
    {
        byte[] deflate;
        using (var compressed = new MemoryStream())
        {
            using (var deflater = new DeflateStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
            {
                deflater.Write(raw, 0, raw.Length);
            }

            deflate = compressed.ToArray();
        }

        var result = new byte[2 + deflate.Length + 4];
        result[0] = 0x78;
        result[1] = 0x01;
        Buffer.BlockCopy(deflate, 0, result, 2, deflate.Length);
        WriteBigEndian(result, 2 + deflate.Length, Adler32(raw));
        return result;
    }

    private static void WriteChunk(MemoryStream output, string type, byte[] data)
    {
        var typeBytes = new byte[] { (byte)type[0], (byte)type[1], (byte)type[2], (byte)type[3] };
        var length = new byte[4];
        WriteBigEndian(length, 0, (uint)data.Length);
        output.Write(length, 0, 4);
        output.Write(typeBytes, 0, 4);
        if (data.Length > 0)
        {
            output.Write(data, 0, data.Length);
        }

        var crcInput = new byte[4 + data.Length];
        Buffer.BlockCopy(typeBytes, 0, crcInput, 0, 4);
        Buffer.BlockCopy(data, 0, crcInput, 4, data.Length);
        var crc = new byte[4];
        WriteBigEndian(crc, 0, Crc32(crcInput));
        output.Write(crc, 0, 4);
    }

    private static void WriteBigEndian(byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }

    private static uint Adler32(byte[] data)
    {
        const uint mod = 65521;
        uint a = 1;
        uint b = 0;
        foreach (var value in data)
        {
            a = (a + value) % mod;
            b = (b + a) % mod;
        }

        return (b << 16) | a;
    }

    private static uint Crc32(byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (var value in data)
        {
            crc = (crc >> 8) ^ CrcTable[(crc ^ value) & 0xFF];
        }

        return crc ^ 0xFFFFFFFF;
    }

    private static uint[] BuildCrcTable()
    {
        var table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            var c = n;
            for (var k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            }

            table[n] = c;
        }

        return table;
    }
}
