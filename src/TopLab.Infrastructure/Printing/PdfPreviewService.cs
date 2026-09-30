using System.Diagnostics;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Opens a rendered PDF for on-screen review (WP-01). Copies the file into a
/// dedicated preview folder first, then hands it to the OS default viewer —
/// never the print verb. Failures return <c>Error.Unexpected</c> with an Arabic
/// message; the service does not throw across the Application boundary.
/// </summary>
public sealed class PdfPreviewService : IPdfPreviewService
{
    private readonly Action<string> _openFile;

    public PdfPreviewService()
        : this(OpenWithShell)
    {
    }

    /// <summary>Test seam: substitute the OS open action.</summary>
    public PdfPreviewService(Action<string> openFile)
    {
        _openFile = openFile ?? throw new ArgumentNullException(nameof(openFile));
    }

    public static string PreviewDirectory =>
        Path.Combine(Path.GetTempPath(), "TopLab-PDF-Preview");

    public Task<Result<string>> PreviewAsync(
        string absolutePdfPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(absolutePdfPath) || !Path.IsPathFullyQualified(absolutePdfPath))
            {
                return Task.FromResult(Result<string>.Failure(
                    Error.Unexpected("مسار ملف المعاينة غير صالح.")));
            }

            if (!File.Exists(absolutePdfPath))
            {
                return Task.FromResult(Result<string>.Failure(
                    Error.Unexpected("ملف المعاينة غير موجود.")));
            }

            Directory.CreateDirectory(PreviewDirectory);
            var previewPath = Path.Combine(
                PreviewDirectory,
                $"preview-{Guid.NewGuid():N}{Path.GetExtension(absolutePdfPath)}");

            File.Copy(absolutePdfPath, previewPath, overwrite: false);

            if (!File.Exists(previewPath))
            {
                return Task.FromResult(Result<string>.Failure(
                    Error.Unexpected("تعذّر إنشاء ملف المعاينة.")));
            }

            _openFile(previewPath);
            return Task.FromResult(Result<string>.Success(previewPath));
        }
        catch (Exception)
        {
            return Task.FromResult(Result<string>.Failure(
                Error.Unexpected("تعذّر فتح معاينة التقرير.")));
        }
    }

    private static void OpenWithShell(string path)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                // Explicit open (never print) — review only.
                Verb = "open"
            }
        };

        process.Start();
    }
}
