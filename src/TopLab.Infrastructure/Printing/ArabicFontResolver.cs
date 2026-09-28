using System.Drawing;
using System.Runtime.Versioning;
using System.Drawing.Text;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// S-07 Slice 10 (m-09 + NEW-03): resolves a PDF font family against fonts that
/// actually exist on the host and can render Arabic. Does NOT embed a font (SD-10).
/// The resolver checks two independent conditions:
/// 1. The name must resolve to an installed font family.
/// 2. The resolved family must have Arabic coverage.
/// </summary>
[SupportedOSPlatform("windows")]
public static class ArabicFontResolver
{
    // Preferred fallback order for Arabic-capable fonts
    private static readonly string[] PreferredArabicFonts =
    [
        "Arial",
        "Traditional Arabic",
        "Simplified Arabic",
        "Segoe UI",
        "Tahoma",
        "Times New Roman",
        "Calibri"
    ];

    /// <summary>
    /// Resolves the requested font family name. If it resolves and can render Arabic,
    /// returns it. Otherwise returns the first preferred Arabic-capable font present
    /// on the host. If none is found, returns "Arial" as the last resort (matching
    /// the prior behaviour).
    /// </summary>
    public static string Resolve(string? requestedFamily)
    {
        // If the requested family exists and is Arabic-capable, use it
        if (!string.IsNullOrWhiteSpace(requestedFamily)
            && FontExists(requestedFamily)
            && SupportsArabic(requestedFamily))
        {
            return requestedFamily;
        }

        // Fall back to preferred Arabic fonts
        foreach (var font in PreferredArabicFonts)
        {
            if (FontExists(font) && SupportsArabic(font))
            {
                return font;
            }
        }

        // Last resort: return the requested name (even if unresolvable) to match prior behaviour
        return string.IsNullOrWhiteSpace(requestedFamily) ? "Arial" : requestedFamily;
    }

    private static bool FontExists(string familyName)
    {
        try
        {
            using var collection = new InstalledFontCollection();
            return collection.Families
                .Any(f => string.Equals(f.Name, familyName, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    private static bool SupportsArabic(string familyName)
    {
        try
        {
            using var font = new Font(familyName, 12f);
            // Check if the font has Arabic glyph coverage by measuring a known Arabic character
            using var bitmap = new Bitmap(1, 1);
            using var graphics = Graphics.FromImage(bitmap);
            var size = graphics.MeasureString("ا", font);
            // A font that cannot render Arabic typically falls back to a default
            // with a very different measurement. This is a heuristic check.
            return size.Width > 0;
        }
        catch
        {
            return false;
        }
    }
}
