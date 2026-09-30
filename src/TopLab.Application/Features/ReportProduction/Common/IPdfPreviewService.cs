using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.ReportProduction.Common;

/// <summary>
/// Opens a rendered PDF for on-screen review before any print dispatch.
/// Never sends the document to a printer. Returns the preview file path on
/// success; on failure returns <see cref="Error.Unexpected"/> with an Arabic message.
/// </summary>
public interface IPdfPreviewService
{
    Task<Result<string>> PreviewAsync(
        string absolutePdfPath,
        CancellationToken cancellationToken = default);
}
