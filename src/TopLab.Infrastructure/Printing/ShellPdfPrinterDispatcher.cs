using System.Diagnostics;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Dispatches a rendered report PDF to the OS print association (shell "print"
/// verb). Failures bubble up and are translated to <c>Error.Unexpected</c> by
/// <see cref="ReportPrintingService"/> — the service never throws.
/// </summary>
public sealed class ShellPdfPrinterDispatcher : IPdfPrinterDispatcher
{
    public async Task DispatchAsync(string pdfFilePath, string printerName, CancellationToken cancellationToken = default)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = pdfFilePath,
                Verb = "print",
                UseShellExecute = true
            }
        };

        process.Start();
        await process.WaitForExitAsync(cancellationToken);
    }
}