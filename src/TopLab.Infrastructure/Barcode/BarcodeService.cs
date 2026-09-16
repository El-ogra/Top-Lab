using System.Globalization;
using System.IO.Compression;
using System.Text;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Printing;

namespace TopLab.Infrastructure.Barcode;

/// <summary>
/// <see cref="IBarcodeService"/> implementation (S-01 slice S1).
/// Mirrors <c>ReportPrintingService</c> line-for-line in spirit: reads
/// <c>SystemSettings</c> (single row, PK=1) at print time, builds the payload
/// (identifier, optionally + print datetime when
/// <c>PrintDateTimeOnTubeBarcode</c>), renders a Code-128 label, embeds it into
/// a minimal PDF page sized for label stock, and dispatches it to the printer
/// routed through <c>PrinterAssignment</c> (OutputType = Barcode).
/// Every failure surfaces as <c>Error.Unexpected</c> — never throws.
/// </summary>
public sealed class BarcodeService : IBarcodeService
{
    private readonly IApplicationDbContext _db;
    private readonly BarcodeLabelRenderer _renderer;
    private readonly IDateTimeProvider _clock;
    private readonly IPdfPrinterDispatcher _dispatcher;

    public BarcodeService(
        IApplicationDbContext db,
        BarcodeLabelRenderer renderer,
        IDateTimeProvider clock,
        IPdfPrinterDispatcher dispatcher)
    {
        _db = db;
        _renderer = renderer;
        _clock = clock;
        _dispatcher = dispatcher;
    }

    public async Task<Result> PrintBarcodeAsync(string value, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result.Failure(Error.Unexpected("قيمة الباركود غير صالحة."));
            }

            var systemSettings = _db.Set<SystemSettings>().SingleOrDefault(s => s.Id == 1);
            if (systemSettings is null)
            {
                return Result.Failure(Error.Unexpected("سجل إعدادات النظام مفقود."));
            }

            var assignment = _db.Set<PrinterAssignment>().FirstOrDefault(a => a.OutputType == PrinterOutputType.Barcode);
            if (assignment is null)
            {
                return Result.Failure(Error.Unexpected("لم يتم تعيين طابعة للباركود."));
            }

            var payload = systemSettings.PrintDateTimeOnTubeBarcode
                ? $"{value} {_clock.UtcNow.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}"
                : value;

            var label = _renderer.Render(payload);
            var pdfBytes = BuildLabelPdf(label, payload);

            var pdfPath = Path.Combine(Path.GetTempPath(), $"TopLabBarcode-{Guid.NewGuid():N}.pdf");
            await File.WriteAllBytesAsync(pdfPath, pdfBytes, cancellationToken);
            await _dispatcher.DispatchAsync(pdfPath, assignment.PrinterName, cancellationToken);

            // The temp PDF is intentionally left in the OS temp directory.
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Result.Failure(Error.Unexpected("تعذر طباعة الباركود."));
        }
    }

    internal static byte[] BuildLabelPdf(BarcodeLabelData label, string humanReadableId)
    {
        // Label stock page: 4in x 2in at 72pt/in.
        const int pageWidth = 288;
        const int pageHeight = 144;

        var rgb = ToRgb(label.Pixels);
        var compressed = Deflate(rgb);

        // Fit the bitmap into the page with 12pt margins, reserving 20pt at the
        // bottom for the human-readable identifier line.
        const double margin = 12;
        const double textReserve = 20;
        var scale = Math.Min(
            (pageWidth - 2 * margin) / label.Width,
            (pageHeight - 2 * margin - textReserve) / label.Height);
        var drawWidth = label.Width * scale;
        var drawHeight = label.Height * scale;
        var drawX = (pageWidth - drawWidth) / 2;
        var drawY = margin + textReserve;

        var content = new StringBuilder();
        content.Append(CultureInfo.InvariantCulture,
            $"q {drawWidth:F2} 0 0 {drawHeight:F2} {drawX:F2} {drawY:F2} cm /Im0 Do Q ");
        content.Append("BT /F1 9 Tf ");
        content.Append(CultureInfo.InvariantCulture, $"{margin:F2} {margin:F2} Td ");
        content.Append($"({Escape(ToAscii(humanReadableId))}) Tj ET");
        var contentBytes = Encoding.ASCII.GetBytes(content.ToString());

        var sb = new StringBuilder();
        sb.Append("%PDF-1.4\n");
        var offsets = new List<int>();

        void AddObject(int number, string body)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(sb.ToString()));
            sb.Append($"{number} 0 obj\n{body}\nendobj\n");
        }

        AddObject(1, "<< /Type /Catalog /Pages 2 0 R >>");
        AddObject(2, "<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
        AddObject(3, $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {pageWidth} {pageHeight}] /Contents 5 0 R /Resources << /XObject << /Im0 4 0 R >> /Font << /F1 6 0 R >> >> >>");

        offsets.Add(Encoding.ASCII.GetByteCount(sb.ToString()));
        sb.Append($"4 0 obj\n<< /Type /XObject /Subtype /Image /Width {label.Width} /Height {label.Height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Length {compressed.Length} /Filter /FlateDecode >>\nstream\n");
        var streamHeader = Encoding.ASCII.GetBytes(sb.ToString());

        var contentHeader = Encoding.ASCII.GetBytes("\nendstream\nendobj\n");
        var contentObjHeader = Encoding.ASCII.GetBytes(
            $"5 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
        var contentObjFooter = Encoding.ASCII.GetBytes("\nendstream\nendobj\n");
        var fontObj = Encoding.ASCII.GetBytes("6 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");

        // Record the remaining object offsets so the xref table covers all 7 entries.
        offsets.Add(streamHeader.Length + compressed.Length + contentHeader.Length);
        offsets.Add(offsets[4] + contentObjHeader.Length + contentBytes.Length + contentObjFooter.Length);

        // Layout: header + image bytes + contentHeader-close + content obj + footer.
        var contentFooter = Concat(contentObjFooter, fontObj);
        var xrefPosition = streamHeader.Length + compressed.Length + contentHeader.Length
            + contentObjHeader.Length + contentBytes.Length + contentFooter.Length;
        var trailer = Encoding.ASCII.GetBytes(
            $"xref\n0 7\n0000000000 65535 f \n{string.Join("", offsets.Select(o => $"{o:D10} 00000 n \n"))}" +
            $"trailer\n<< /Size 7 /Root 1 0 R >>\nstartxref\n{xrefPosition}\n%%EOF\n");

        var result = new byte[xrefPosition + trailer.Length];
        var pos = 0;
        Buffer.BlockCopy(streamHeader, 0, result, pos, streamHeader.Length);
        pos += streamHeader.Length;
        Buffer.BlockCopy(compressed, 0, result, pos, compressed.Length);
        pos += compressed.Length;
        Buffer.BlockCopy(contentHeader, 0, result, pos, contentHeader.Length);
        pos += contentHeader.Length;
        Buffer.BlockCopy(contentObjHeader, 0, result, pos, contentObjHeader.Length);
        pos += contentObjHeader.Length;
        Buffer.BlockCopy(contentBytes, 0, result, pos, contentBytes.Length);
        pos += contentBytes.Length;
        Buffer.BlockCopy(contentFooter, 0, result, pos, contentFooter.Length);
        pos += contentFooter.Length;
        Buffer.BlockCopy(trailer, 0, result, pos, trailer.Length);
        return result;
    }

    private static byte[] Concat(byte[] first, byte[] second)
    {
        var result = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, result, 0, first.Length);
        Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
        return result;
    }

    private static byte[] ToRgb(byte[] rgba)
    {
        var pixelCount = rgba.Length / 4;
        var rgb = new byte[pixelCount * 3];
        for (int i = 0, j = 0; i < pixelCount; i++, j += 3)
        {
            rgb[j] = rgba[i * 4];
            rgb[j + 1] = rgba[i * 4 + 1];
            rgb[j + 2] = rgba[i * 4 + 2];
        }

        return rgb;
    }

    private static byte[] Deflate(byte[] data)
    {
        using var output = new MemoryStream();
        using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            deflate.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }

    private static string Escape(string value)
    {
        return value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }

    private static string ToAscii(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            sb.Append(ch <= 126 ? ch : '?');
        }

        return sb.ToString();
    }
}
