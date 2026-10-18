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

    private ReportItemTemplate()
    {
    }

    private ReportItemTemplate(
        string itemKey,
        string label,
        string? referenceValueExpression,
        bool showHide,
        string? listMembership,
        bool freeTextOverride)
    {
        ItemKey = itemKey;
        Label = label;
        ReferenceValueExpression = referenceValueExpression;
        ShowHide = showHide;
        ListMembership = listMembership;
        FreeTextOverride = freeTextOverride;
    }

    public static ReportItemTemplate Create(
        string itemKey,
        string label,
        string? referenceValueExpression = null,
        bool showHide = true,
        string? listMembership = null,
        bool freeTextOverride = false)
    {
        return new ReportItemTemplate(
            NormalizeRequired(itemKey, MaxItemKeyLength, nameof(itemKey)),
            NormalizeRequired(label, MaxLabelLength, nameof(label)),
            NormalizeOptional(referenceValueExpression, MaxReferenceValueExpressionLength, nameof(referenceValueExpression)),
            showHide,
            NormalizeOptional(listMembership, MaxListMembershipLength, nameof(listMembership)),
            freeTextOverride);
    }

    /// <summary>Updates the editable values; the natural key is immutable.</summary>
    public void Update(
        string label,
        string? referenceValueExpression,
        bool showHide,
        string? listMembership,
        bool freeTextOverride)
    {
        Label = NormalizeRequired(label, MaxLabelLength, nameof(label));
        ReferenceValueExpression = NormalizeOptional(referenceValueExpression, MaxReferenceValueExpressionLength, nameof(referenceValueExpression));
        ShowHide = showHide;
        ListMembership = NormalizeOptional(listMembership, MaxListMembershipLength, nameof(listMembership));
        FreeTextOverride = freeTextOverride;
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
}
