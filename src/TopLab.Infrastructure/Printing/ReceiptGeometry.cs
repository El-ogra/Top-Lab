using TopLab.Domain.Common.Enums;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Explicit page-geometry helpers for receipt rendering. QuestPDF uses
/// PostScript points (1 pt = 1/72 in); this is the receipt analogue of
/// <see cref="PageSizeMapper"/> (which is report-scoped).
/// </summary>
public static class ReceiptGeometry
{
    /// <summary>A5 portrait: 148×210 mm = 420×595 pt — the receipt paper size.</summary>
    public const float A5WidthPt = 420f;

    public const float A5HeightPt = 595f;

    /// <summary>Largest fraction of the printable content height that the lab logo
    /// may occupy (GAP-010). Bounds an over-tall logo so it can neither exhaust
    /// the page nor collide with pre-printed stationery; wide logos (the normal
    /// case) are width-bound long before they reach this cap.</summary>
    public const float LogoMaxHeightFraction = 1f / 3f;

    private const float CmToPt = 28.3465f;

    /// <summary>Printable width of the receipt body, i.e. the page width minus the
    /// left/right margins that <see cref="ReceiptPdfWriter"/> applies to every page.</summary>
    public static float ContentWidthPt(float leftMarginPt, float rightMarginPt)
        => Math.Max(0f, A5WidthPt - Math.Max(0f, leftMarginPt) - Math.Max(0f, rightMarginPt));

    /// <summary>Printable height of the receipt body: the page height minus the
    /// top margin (which doubles as the pre-printed-paper clearance knob) and the
    /// bottom margin.</summary>
    public static float ContentHeightPt(float topMarginPt, float bottomMarginPt)
        => Math.Max(0f, A5HeightPt - Math.Max(0f, topMarginPt) - Math.Max(0f, bottomMarginPt));

    /// <summary>Largest fraction of the printable content height that the logo may
    /// occupy, in points. Degenerate geometry never yields a negative box.</summary>
    public static float LogoMaxHeightPt(float topMarginPt, float bottomMarginPt)
        => ContentHeightPt(topMarginPt, bottomMarginPt) * LogoMaxHeightFraction;

    public static float CmToPoints(decimal cm) => (float)cm * CmToPt;

    /// <summary>
    /// The largest top margin (cm) the receipt writer will actually apply. Above
    /// it the A5 printable box (page height less top margin less bottom margin) is
    /// too small for the receipt body, and QuestPDF throws a layout exception:
    /// no receipt could be produced at all. The receipt settings screen allows up
    /// to 30 cm, so the excess is clamped here rather than failing the print.
    /// </summary>
    public const decimal MaxTopMarginCm = 10m;

    /// <summary>
    /// True when the given top margin still leaves printable height on the page.
    /// Exposed so callers and tests can state the invariant without duplicating
    /// the arithmetic.
    /// </summary>
    public static bool LeavesPrintableHeight(decimal topMarginCm)
        => CmToPoints(topMarginCm) + CmToPoints(BottomMarginCm) < A5HeightPt;

    /// <summary>The bottom margin the receipt applies to every page.</summary>
    public const decimal BottomMarginCm = 1m;
}

