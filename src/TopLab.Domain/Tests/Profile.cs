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

    public string Name { get; private set; } = default!;

    public TestId TestId { get; private set; } = default!;

    public decimal FixedPrice { get; private set; }

    public bool IsActive { get; private set; }

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