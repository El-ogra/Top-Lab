namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Seam between <see cref="ReportPrintingService"/> and the OS print pipeline so
/// tests can stub dispatch (unavailable printer → <c>Error.Unexpected</c>). The
/// real implementation hands the rendered PDF to the operating system's print
/// association; per-printer spooler binding lives at the Presentation layer
/// (Reporting &amp; Printing Blueprint §3.3).
/// </summary>
public interface IPdfPrinterDispatcher
{
    Task DispatchAsync(string pdfFilePath, string printerName, CancellationToken cancellationToken = default);
}