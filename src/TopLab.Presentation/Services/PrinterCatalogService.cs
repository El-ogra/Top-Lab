namespace TopLab.Presentation.Services;

/// <summary>
/// Enumerates installed printers via <c>System.Drawing.Printing</c>. The caller keeps
/// a free-text fallback so network/offline printers that do not enumerate can still
/// be assigned by typing their name.
/// </summary>
public sealed class PrinterCatalogService : IPrinterCatalogService
{
    public IReadOnlyList<string> GetInstalledPrinters()
    {
        try
        {
            var names = System.Drawing.Printing.PrinterSettings.InstalledPrinters.Cast<string>().ToList();
            return names;
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}