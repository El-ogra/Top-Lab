using System.Globalization;
using System.Text;
using System.Text.Json;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

public sealed class ReportPdfWriter : IReportPdfWriter
{
    public async Task WritePdfAsync(
        string absolutePath,
        ReportPrintEnvelope envelope,
        ReportSettings reportSettings,
        SystemSettings systemSettings,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(reportSettings);
        ArgumentNullException.ThrowIfNull(systemSettings);

        var bytes = BuildPdf(envelope, reportSettings, systemSettings);

        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        await using var stream = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await stream.WriteAsync(bytes, cancellationToken);
    }

    internal static byte[] BuildPdf(
        ReportPrintEnvelope envelope,
        ReportSettings reportSettings,
        SystemSettings systemSettings)
    {
        var lines = new List<string>
        {
            $"TopLab {envelope.ReportKind} Report"
        };

        var printLabId = systemSettings.PrintLabIdInsteadOfPatientId && !string.IsNullOrWhiteSpace(CurrentLabId(envelope));
        lines.Add(printLabId
            ? $"LabId: {ToAscii(CurrentLabId(envelope))}"
            : $"PatientId: {CurrentPatientId(envelope)}");
        lines.Add($"Name: {ToAscii(CurrentFullName(envelope))}");
        lines.Add($"Paper: {reportSettings.PaperSize}");
        lines.Add($"TopSpace: {reportSettings.ReportTopSpaceCm.ToString(CultureInfo.InvariantCulture)}");

        if (reportSettings.HeaderFooterMode != TopLab.Domain.Common.Enums.HeaderFooterMode.None)
        {
            lines.Add($"HeaderFooter: {reportSettings.HeaderFooterMode}");
        }

        if (reportSettings.DoctorSignatureEnabled)
        {
            lines.Add("Doctor Signature: Yes");
        }

        switch (envelope.ReportKind)
        {
            case ReportPrintEnvelope.Combined:
                var combined = JsonSerializer.Deserialize<CombinedReportDto>(envelope.ReportJson)
                    ?? throw new InvalidOperationException("Report payload is empty.");
                foreach (var line in combined.Lines)
                {
                    RenderTestLine(lines, line.TestCode, line.TestName, line.ResultValue);
                    if (line.ResultFlag is { } flag)
                    {
                        lines.Add($"  Flag: {flag}");
                    }

                    if (!string.IsNullOrWhiteSpace(line.FrozenRangeText))
                    {
                        lines.Add($"  Range: {ToAscii(line.FrozenRangeText)}");
                    }

                    foreach (var profile in line.ProfileLines)
                    {
                        var unit = string.IsNullOrWhiteSpace(profile.Unit) ? string.Empty : $" {ToAscii(profile.Unit)}";
                        lines.Add($"  {ToAscii(profile.AnalyteName)}: {ToAscii(profile.ResultValue ?? "-")}{unit}");
                        if (profile.Flag is { } profileFlag)
                        {
                            lines.Add($"    Flag: {profileFlag}");
                        }
                    }

                    if (line.Culture is { } culture)
                    {
                        var summary = string.Join(", ", new[]
                        {
                            culture.Sample, culture.OrganismA, culture.OrganismB,
                            culture.OrganismC, culture.CultureCondition, culture.ColonyCount
                        }.Where(s => !string.IsNullOrWhiteSpace(s)));
                        if (summary.Length > 0)
                        {
                            lines.Add($"  Culture: {ToAscii(summary)}");
                        }
                    }
                }

                break;

            case ReportPrintEnvelope.Blank:
                var blank = JsonSerializer.Deserialize<BlankReportDto>(envelope.ReportJson)
                    ?? throw new InvalidOperationException("Report payload is empty.");
                lines.Add($"Sex: {ToAscii(blank.Sex)}");
                lines.Add($"Age: {blank.AgeValue} {ToAscii(blank.AgeUnit)}");
                if (!string.IsNullOrWhiteSpace(blank.TreatingDoctorName))
                {
                    lines.Add($"Doctor: {ToAscii(blank.TreatingDoctorName)}");
                }

                if (!string.IsNullOrWhiteSpace(blank.ReferralEntityName))
                {
                    lines.Add($"Referral: {ToAscii(blank.ReferralEntityName)}");
                }

                break;

            case ReportPrintEnvelope.History:
                var history = JsonSerializer.Deserialize<PatientHistoryDto>(envelope.ReportJson)
                    ?? throw new InvalidOperationException("Report payload is empty.");
                lines.Add($"SortMode: {history.HistorySortMode}");
                lines.Add($"AutoDisplay: {history.HistoryAutoDisplayEnabled}");
                foreach (var entry in history.Entries)
                {
                    RenderTestLine(lines, entry.TestCode, entry.TestName, entry.ResultValue);
                    lines.Add($"  Reviewed: {(entry.IsReviewed ? "Yes" : "No")}");
                }

                break;

            default:
                throw new InvalidOperationException($"Unknown report kind '{envelope.ReportKind}'.");
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

    private static string? CurrentLabId(ReportPrintEnvelope envelope)
    {
        return envelope.ReportKind switch
        {
            ReportPrintEnvelope.Combined => Deserialize<CombinedReportDto>(envelope)?.LabId,
            ReportPrintEnvelope.Blank => Deserialize<BlankReportDto>(envelope)?.LabId,
            ReportPrintEnvelope.History => Deserialize<PatientHistoryDto>(envelope)?.LabId,
            _ => null
        };
    }

    private static int CurrentPatientId(ReportPrintEnvelope envelope)
    {
        return envelope.ReportKind switch
        {
            ReportPrintEnvelope.Combined => Deserialize<CombinedReportDto>(envelope)?.PatientId ?? 0,
            ReportPrintEnvelope.Blank => Deserialize<BlankReportDto>(envelope)?.PatientId ?? 0,
            ReportPrintEnvelope.History => Deserialize<PatientHistoryDto>(envelope)?.PatientId ?? 0,
            _ => 0
        };
    }

    private static string? CurrentFullName(ReportPrintEnvelope envelope)
    {
        return envelope.ReportKind switch
        {
            ReportPrintEnvelope.Combined => Deserialize<CombinedReportDto>(envelope)?.PatientFullName,
            ReportPrintEnvelope.Blank => Deserialize<BlankReportDto>(envelope)?.PatientFullName,
            ReportPrintEnvelope.History => Deserialize<PatientHistoryDto>(envelope)?.PatientFullName,
            _ => null
        };
    }

    private static T? Deserialize<T>(ReportPrintEnvelope envelope) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(envelope.ReportJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static void RenderTestLine(List<string> lines, string? code, string? name, string? value)
    {
        lines.Add($"{ToAscii(code)} {ToAscii(name)}: {ToAscii(value ?? "-")}");
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