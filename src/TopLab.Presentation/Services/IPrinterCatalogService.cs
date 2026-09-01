namespace TopLab.Presentation.Services;

/// <summary>
/// Supplies the list of printers installable on this workstation for the printer
/// assignment dropdowns. Built on the installed-printer enumeration with free-text
/// fallback for offline/LAN printers that may not enumerate (settled boundary §2.3-9).
/// </summary>
public interface IPrinterCatalogService
{
    IReadOnlyList<string> GetInstalledPrinters();
}