using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

/// <summary>
/// A specialised-profile catalog definition paired 1:1 with a <see cref="Test"/>
/// whose <see cref="ResultKind"/> is <see cref="ResultKind.SpecializedProfile"/>.
/// <c>FixedPrice</c> is immutable and non-negative: profile ordering charges exactly
/// this price via <see cref="TopLab.Domain.Billing.PatientAccountCalculator.ProfileSelectionCharge"/>,
/// never the sum of the profile's constituent analytes (Decision 2).
/// </summary>
public sealed class Profile : AuditableEntity<ProfileId>
{
    public const int MaxNameLength = 150;

    /// <summary>GAP-017: maximum length of a result-font style string.</summary>
    public const int MaxResultFontLength = 50;

    /// <summary>GAP-017: maximum length of a result-colour string (existing colour convention).</summary>
    public const int MaxResultColorLength = 9;

    public string Name { get; private set; } = default!;

    public TestId TestId { get; private set; } = default!;

    public decimal FixedPrice { get; private set; }

    public bool IsActive { get; private set; }

    /// <summary>
    /// GAP-017: font colour applied to this profile's unflagged ("normal") result lines.
    /// Null = no per-profile normal font colour, so the line renders uncoloured.
    /// </summary>
    public string? NormalFont { get; private set; }

    /// <summary>GAP-017: background colour for this profile's normal result lines. Null = none.</summary>
    public string? NormalColor { get; private set; }

    /// <summary>
    /// GAP-017: font colour applied to this profile's flagged result lines when the global
    /// <c>ReportSettings.EnableAbnormalResultColor</c> master switch is on. Null = the global
    /// abnormal font colour applies.
    /// </summary>
    public string? AbnormalFont { get; private set; }

    /// <summary>GAP-017: background colour for this profile's flagged result lines. Null = the global colour.</summary>
    public string? AbnormalBackgroundColor { get; private set; }

    private readonly List<ProfileAnalyte> _analytes = new();

    public IReadOnlyCollection<ProfileAnalyte> Analytes => _analytes;

    private Profile()
    {
    }

    private Profile(ProfileId id, string name, TestId testId, decimal fixedPrice, bool isActive)
        : base(id)
    {
        Name = name;
        TestId = testId;
        FixedPrice = fixedPrice;
        IsActive = isActive;
    }

    public static Profile Create(
        ProfileId id,
        string name,
        TestId testId,
        ResultKind testResultKind,
        decimal fixedPrice,
        bool isActive = true)
    {
        Guard(name, fixedPrice);

        if (testResultKind != ResultKind.SpecializedProfile)
        {
            throw new ArgumentException("A Profile can only pair with a specialised-profile test.", nameof(testResultKind));
        }

        return new Profile(id, name.Trim(), testId, fixedPrice, isActive);
    }

    public void UpdateName(string name)
    {
        Guard(name, FixedPrice);

        Name = name.Trim();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Reactivate()
    {
        IsActive = true;
    }

    /// <summary>
    /// GAP-017: sets this profile's per-profile result styling. Every value is optional;
    /// null (or an empty/whitespace string) clears that override so the global behaviour applies.
    /// <para>
    /// <b>Precedence (applied by the report writers, not here).</b> For an abnormal (flagged)
    /// result: when <c>ReportSettings.EnableAbnormalResultColor</c> is true and this profile
    /// carries an abnormal font/background value, that value wins over the global
    /// <c>ReportSettings.AbnormalResultFontColor</c>/<c>AbnormalResultBackgroundColor</c>;
    /// when this profile carries no abnormal value the global colour applies; and when
    /// <c>EnableAbnormalResultColor</c> is false no abnormal colouring is applied at all,
    /// profile override included (master switch).
    /// </para>
    /// <para>
    /// For a normal (unflagged) result there is no global colour: the profile's
    /// <see cref="NormalFont"/>/<see cref="NormalColor"/> apply when present, and the line
    /// renders uncoloured when they are absent.
    /// </para>
    /// </summary>
    public void SetResultStyling(
        string? normalFont,
        string? normalColor,
        string? abnormalFont,
        string? abnormalBackgroundColor)
    {
        NormalFont = NormalizeStyle(normalFont, MaxResultFontLength, nameof(normalFont));
        NormalColor = NormalizeStyle(normalColor, MaxResultColorLength, nameof(normalColor));
        AbnormalFont = NormalizeStyle(abnormalFont, MaxResultFontLength, nameof(abnormalFont));
        AbnormalBackgroundColor = NormalizeStyle(abnormalBackgroundColor, MaxResultColorLength, nameof(abnormalBackgroundColor));
    }

    private static string? NormalizeStyle(string? value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Result styling must be at most {maxLength} characters.", parameterName);
        }

        return trimmed;
    }

    public void AddAnalyte(ProfileAnalyte link)
    {
        ArgumentNullException.ThrowIfNull(link);

        if (link.ProfileId != Id)
        {
            throw new ArgumentException("Link must belong to this profile.", nameof(link));
        }

        if (_analytes.Any(a => a.AnalyteId == link.AnalyteId))
        {
            throw new InvalidOperationException("Analyte is already linked to this profile.");
        }

        _analytes.Add(link);
    }

    public void RemoveAnalyte(AnalyteId analyteId)
    {
        ArgumentNullException.ThrowIfNull(analyteId);

        var link = _analytes.FirstOrDefault(a => a.AnalyteId == analyteId);
        if (link is null)
        {
            throw new InvalidOperationException("Analyte is not linked to this profile.");
        }

        _analytes.Remove(link);
    }

    private static void Guard(string name, decimal fixedPrice)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name required.");
        }

        if (name.Trim().Length > MaxNameLength)
        {
            throw new ArgumentException($"Name must be at most {MaxNameLength} characters.");
        }

        if (fixedPrice < 0)
        {
            throw new ArgumentException("FixedPrice must be >= 0.", nameof(fixedPrice));
        }
    }
}