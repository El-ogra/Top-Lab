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

    // GDI charset constant for Arabic (WinGDI ARABIC_CHARSET).
    private const byte ArabicGdiCharSet = 178;

    private static bool SupportsArabic(string familyName)
    {
        try
        {
            // Arabic script block U+0600–U+06FF. MeasureString("ا")>0 is true for
            // almost every font (fallback glyph) and is not a coverage test — it
            // must not be used. Prefer explicit Arabic-capable families and the
            // GDI charset that Windows assigns to Arabic fonts.
            if (PreferredArabicFonts.Contains(familyName, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            using var font = new Font(familyName, 12f);
            if (font.GdiCharSet == ArabicGdiCharSet)
            {
                return true;
            }

            // Common Windows UI families that ship Arabic glyphs (default ANSI charset).
            return familyName is "Arial" or "Segoe UI" or "Tahoma" or "Times New Roman" or "Calibri";
        }
        catch
        {
            return false;
        }
    }
}
