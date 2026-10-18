using TopLab.Domain.Common.Enums;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Explicit page-geometry helpers for report rendering. No raw point math in
/// callers (WP-01 step 4). QuestPDF uses PostScript points (1 pt = 1/72 in).
/// </summary>
public static class PageSizeMapper
{
    /// <summary>A4 portrait: 210×297 mm = 595×842 pt.</summary>
    public const float A4WidthPt = 595f;

    public const float A4HeightPt = 842f;

    /// <summary>A5 portrait: 148×210 mm = 420×595 pt.</summary>
    public const float A5WidthPt = 420f;

    public const float A5HeightPt = 595f;

    private const float CmToPt = 28.3465f;

    public static (float Width, float Height) ToPoints(PaperSize paperSize)
    {
        return paperSize == PaperSize.A5
            ? (A5WidthPt, A5HeightPt)
            : (A4WidthPt, A4HeightPt);
    }

    public static float CmToPoints(decimal cm)
    {
        return (float)cm * CmToPt;
    }

    /// <summary>
    /// Largest fraction of the printable content height that a single header or
    /// footer image may occupy. Keeps a logo from colliding with pre-printed
    /// stationery or pushing the report body off the page, on either paper
    /// choice. Wide logos (the normal case) are unaffected because they are
    /// width-bound long before they reach this cap.
    /// </summary>
    public const float HeaderFooterImageMaxHeightFraction = 1f / 3f;

    /// <summary>
    /// Aspect-preserving box for a header/footer image: at most the full content
    /// width and at most <paramref name="maxHeightPt"/> tall. Rendering an image
    /// unconstrained lets an over-tall logo exhaust the page and aborts PDF
    /// generation with a QuestPDF layout exception, so callers must bound it.
    /// </summary>
    public static (float Width, float MaxHeight) FitHeaderFooterImage(
        PaperSize paperSize, decimal marginLeftCm, float maxHeightPt)
    {
        var (width, _) = ToPoints(paperSize);
        var contentWidth = width - (2f * CmToPoints(marginLeftCm));
        return (Math.Max(0f, contentWidth), Math.Max(0f, maxHeightPt));
    }
}
