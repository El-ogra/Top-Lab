using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

/// <summary>
/// The current reference-range aggregate owned by one <see cref="Analyte"/>. Exactly
/// one aggregate exists per analyte (enforced by a unique index on
/// <c>AnalyteId</c> and the <see cref="Analyte.AttachCurrentRange"/> guard). The
/// clinical bands live beneath this aggregate as <see cref="AnalyteReferenceRangeBand"/>.
/// Result entry freezes the matching band into a per-result snapshot; this aggregate
/// is never read by reports/reprints.
/// </summary>
public sealed class AnalyteReferenceRange : Entity<AnalyteReferenceRangeId>
{
    public AnalyteId AnalyteId { get; private set; } = default!;

    private readonly List<AnalyteReferenceRangeBand> _bands = new();

    public IReadOnlyCollection<AnalyteReferenceRangeBand> Bands => _bands;

    private AnalyteReferenceRange()
    {
    }

    private AnalyteReferenceRange(AnalyteReferenceRangeId id, AnalyteId analyteId)
        : base(id)
    {
        AnalyteId = analyteId;
    }

    public static AnalyteReferenceRange Create(AnalyteReferenceRangeId id, AnalyteId analyteId)
    {
        ArgumentNullException.ThrowIfNull(analyteId);

        return new AnalyteReferenceRange(id, analyteId);
    }

    /// <summary>The one band matching the given sex/age-unit/age, or null. Age-unit sensitive (BR-04).</summary>
    public AnalyteReferenceRangeBand? Match(Sex? sex, AgeUnit ageUnit, int ageValue)
    {
        return _bands
            .Where(b => b.Matches(sex, ageUnit, ageValue))
            .OrderBy(b => b.AgeMin)
            .ThenBy(b => b.Id.Value)
            .FirstOrDefault();
    }

    public void AddBand(AnalyteReferenceRangeBand band)
    {
        ArgumentNullException.ThrowIfNull(band);

        if (band.AnalyteReferenceRangeId != Id)
        {
            throw new ArgumentException("Band must belong to this range aggregate.", nameof(band));
        }

        _bands.Add(band);
    }
}