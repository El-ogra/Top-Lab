using System.Globalization;
using System.Text;

namespace TopLab.Domain.Settings;

/// <summary>
/// GAP-014 (Phase 7): one editable report-item template row. Keyed by the
/// natural <see cref="ItemKey"/> token (see <see cref="ReportItemKeys"/>) with
/// no surrogate identifier — the closest existing precedent is
/// <see cref="EnvelopePrintItemPosition"/> (string <c>ItemName</c> key).
/// </summary>
/// <remarks>
/// Deliberately a plain, non-audited configuration entity (mirroring the other
/// <c>TopLab.Domain.Settings</c> rows) rather than an <c>AuditableEntity</c>:
/// report-item configuration is a settings concern and the codebase does not
/// audit its settings rows, so audit columns would be an unrelated schema
/// addition. The natural string key is preserved intentionally; adding a
/// surrogate id would contradict the required key design.
/// </remarks>
public sealed class ReportItemTemplate
{
    public const int MaxItemKeyLength = 100;
    public const int MaxLabelLength = 200;
    public const int MaxReferenceValueExpressionLength = 500;
    public const int MaxListMembershipLength = 100;

    /// <summary>GAP-015: storage headroom for the marker glyph (the content rule is one text element).</summary>
    public const int MaxMarkerSymbolLength = 10;

    /// <summary>GAP-015: accepted line-spacing multipliers (QuestPDF line-height factors).</summary>
    public const decimal MinLineSpacing = 0.5m;

    public const decimal MaxLineSpacing = 10.0m;

    /// <summary>GAP-015: accepted additive indent, in centimetres from the left page margin.</summary>
    public const decimal MinLeftOffsetCm = 0.0m;

    public const decimal MaxLeftOffsetCm = 10.0m;

    /// <summary>Natural primary key; one of <see cref="ReportItemKeys"/>.</summary>
    public string ItemKey { get; private set; } = default!;

    /// <summary>Display label / free-text payload; required and non-blank.</summary>
    public string Label { get; private set; } = default!;

    /// <summary>Optional reference-value expression / prefix override.</summary>
    public string? ReferenceValueExpression { get; private set; }

    /// <summary>Whether the item is rendered. Non-null; defaults to <c>true</c>.</summary>
    public bool ShowHide { get; private set; }

    /// <summary>Optional selection-list membership token.</summary>
    public string? ListMembership { get; private set; }

    /// <summary>Whether the item is rendered as free text. Non-null; defaults to <c>false</c>.</summary>
    public bool FreeTextOverride { get; private set; }

    /// <summary>
    /// GAP-015: per-item line spacing as a line-height multiplier. <c>null</c> =
    /// the composer's existing default spacing (unchanged rendering).
    /// </summary>
    public decimal? LineSpacing { get; private set; }

    /// <summary>
    /// GAP-015: additive indent in centimetres, applied on top of
    /// <see cref="ReportSettings.PageMarginLeftCm"/>. <c>null</c> = today's
    /// behaviour (the item starts at the page margin).
    /// </summary>
    public decimal? LeftOffsetCm { get; private set; }

    /// <summary>
    /// GAP-015: single visible character drawn immediately before the item's
    /// content. <c>null</c> = no marker (today's behaviour).
    /// </summary>
    public string? MarkerSymbol { get; private set; }

    private ReportItemTemplate()
    {
    }

    private ReportItemTemplate(
        string itemKey,
        string label,
        string? referenceValueExpression,
        bool showHide,
        string? listMembership,
        bool freeTextOverride,
        decimal? lineSpacing,
        decimal? leftOffsetCm,
        string? markerSymbol)
    {
        ItemKey = itemKey;
        Label = label;
        ReferenceValueExpression = referenceValueExpression;
        ShowHide = showHide;
        ListMembership = listMembership;
        FreeTextOverride = freeTextOverride;
        LineSpacing = lineSpacing;
        LeftOffsetCm = leftOffsetCm;
        MarkerSymbol = markerSymbol;
    }

    public static ReportItemTemplate Create(
        string itemKey,
        string label,
        string? referenceValueExpression = null,
        bool showHide = true,
        string? listMembership = null,
        bool freeTextOverride = false,
        decimal? lineSpacing = null,
        decimal? leftOffsetCm = null,
        string? markerSymbol = null)
    {
        return new ReportItemTemplate(
            NormalizeRequired(itemKey, MaxItemKeyLength, nameof(itemKey)),
            NormalizeRequired(label, MaxLabelLength, nameof(label)),
            NormalizeOptional(referenceValueExpression, MaxReferenceValueExpressionLength, nameof(referenceValueExpression)),
            showHide,
            NormalizeOptional(listMembership, MaxListMembershipLength, nameof(listMembership)),
            freeTextOverride,
            NormalizeLineSpacing(lineSpacing),
            NormalizeLeftOffset(leftOffsetCm),
            NormalizeMarkerSymbol(markerSymbol));
    }

    /// <summary>Updates the editable values; the natural key is immutable.</summary>
    public void Update(
        string label,
        string? referenceValueExpression,
        bool showHide,
        string? listMembership,
        bool freeTextOverride,
        decimal? lineSpacing = null,
        decimal? leftOffsetCm = null,
        string? markerSymbol = null)
    {
        Label = NormalizeRequired(label, MaxLabelLength, nameof(label));
        ReferenceValueExpression = NormalizeOptional(referenceValueExpression, MaxReferenceValueExpressionLength, nameof(referenceValueExpression));
        ShowHide = showHide;
        ListMembership = NormalizeOptional(listMembership, MaxListMembershipLength, nameof(listMembership));
        FreeTextOverride = freeTextOverride;
        LineSpacing = NormalizeLineSpacing(lineSpacing);
        LeftOffsetCm = NormalizeLeftOffset(leftOffsetCm);
        MarkerSymbol = NormalizeMarkerSymbol(markerSymbol);
    }

    private static string NormalizeRequired(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} is required.", paramName);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} must be at most {maxLength} characters.", paramName);
        }

        return trimmed;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} must be at most {maxLength} characters.", paramName);
        }

        return trimmed;
    }

    /// <summary>
    /// GAP-015: <c>null</c> keeps the composer's existing spacing; a value must
    /// be a usable line-height multiplier.
    /// </summary>
    private static decimal? NormalizeLineSpacing(decimal? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value < MinLineSpacing || value > MaxLineSpacing)
        {
            throw new ArgumentOutOfRangeException(
                nameof(LineSpacing),
                value.Value,
                $"Line spacing must be between {MinLineSpacing} and {MaxLineSpacing}.");
        }

        return value;
    }

    /// <summary>GAP-015: <c>null</c> keeps the item at the page margin.</summary>
    private static decimal? NormalizeLeftOffset(decimal? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value < MinLeftOffsetCm || value > MaxLeftOffsetCm)
        {
            throw new ArgumentOutOfRangeException(
                nameof(LeftOffsetCm),
                value.Value,
                $"Left offset must be between {MinLeftOffsetCm} and {MaxLeftOffsetCm} cm.");
        }

        return value;
    }

    /// <summary>
    /// GAP-015: <c>null</c> draws no marker. A non-null marker must be a single
    /// visible text element (so an emoji or a combining sequence is accepted even
    /// though it spans several UTF-16 code units) and may not be blank or a
    /// control character.
    /// </summary>
    private static string? NormalizeMarkerSymbol(string? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value.Length > MaxMarkerSymbolLength)
        {
            throw new ArgumentException(
                $"{nameof(MarkerSymbol)} must be at most {MaxMarkerSymbolLength} characters.",
                nameof(MarkerSymbol));
        }

        if (string.IsNullOrWhiteSpace(value)
            || new StringInfo(value).LengthInTextElements != 1
            || ContainsControlCharacter(value))
        {
            throw new ArgumentException(
                $"{nameof(MarkerSymbol)} must be exactly one visible character.",
                nameof(MarkerSymbol));
        }

        return value;
    }

    private static bool ContainsControlCharacter(string value)
    {
        foreach (var rune in value.EnumerateRunes())
        {
            if (Rune.IsControl(rune))
            {
                return true;
            }
        }

        return false;
    }
}
