using System.Text;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Infrastructure.Services;

/// <summary>
/// Testable local/LAN PDF writer for the complete eligible patient result report
/// (D2). Generates a minimal valid PDF and writes it to the caller-supplied
/// absolute path. Never overwrites: uses <c>FileMode.CreateNew</c> so a
/// pre-existing target fails instead of being replaced (the Application handler
/// already returns <c>Conflict</c> before calling, this is defense-in-depth).
/// I/O and generation failures throw and are mapped by the handler to
/// <c>Unexpected</c> with no export marks.
/// </summary>
public sealed class PatientReportPdfExporter : IPatientReportPdfExporter
{
    public async Task ExportAsync(string absolutePath, PatientReportPdfData data, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(data);

        var bytes = BuildMinimalPdf(data);

        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        await using var stream = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await stream.WriteAsync(bytes, cancellationToken);
    }

    internal static byte[] BuildMinimalPdf(PatientReportPdfData data)
    {
        var lines = new List<string>
        {
            $"TopLab Patient Report - Patient {data.PatientId}",
            $"Name: {ToAscii(data.PatientFullName)}",
        };

        if (!string.IsNullOrWhiteSpace(data.LabId))
        {
            lines.Add($"LabId: {ToAscii(data.LabId)}");
        }

        foreach (var line in data.Lines)
        {
            lines.Add($"{ToAscii(line.TestCode)} {ToAscii(line.TestName)}: {ToAscii(line.ResultValue ?? "-")}");
            if (line.FrozenRange is not null)
            {
                lines.Add($"  Range: {line.FrozenRange.MinValue} - {line.FrozenRange.MaxValue}");
            }

            foreach (var profile in line.ProfileItems)
            {
                lines.Add($"  {ToAscii(profile)}");
            }

            if (!string.IsNullOrWhiteSpace(line.CultureSummary))
            {
                lines.Add($"  {ToAscii(line.CultureSummary)}");
            }
        }

        var content = new StringBuilder();
        content.Append("BT /F1 12 Tf 50 800 Td 14 TL ");
        foreach (var line in lines)
        {
            content.Append($"({Escape(line)}) Tj T* ");
        }

        content.Append("ET");
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
        AddObject(3, "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>");

        var headerLength = Encoding.ASCII.GetByteCount(sb.ToString());
        offsets.Add(headerLength);
        sb.Append($"4 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
        var streamHeader = Encoding.ASCII.GetBytes(sb.ToString());
        var streamFooter = Encoding.ASCII.GetBytes("\nendstream\nendobj\n5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");

        var xrefPosition = streamHeader.Length + contentBytes.Length + streamFooter.Length;
        var trailer = Encoding.ASCII.GetBytes($"xref\n0 6\n0000000000 65535 f \n{string.Join("", offsets.Select(o => $"{o:D10} 00000 n \n"))}trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n{xrefPosition}\n%%EOF\n");

        var result = new byte[streamHeader.Length + contentBytes.Length + streamFooter.Length + trailer.Length];
        Buffer.BlockCopy(streamHeader, 0, result, 0, streamHeader.Length);
        Buffer.BlockCopy(contentBytes, 0, result, streamHeader.Length, contentBytes.Length);
        Buffer.BlockCopy(streamFooter, 0, result, streamHeader.Length + contentBytes.Length, streamFooter.Length);
        Buffer.BlockCopy(trailer, 0, result, streamHeader.Length + contentBytes.Length + streamFooter.Length, trailer.Length);
        return result;
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
