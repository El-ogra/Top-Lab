using TopLab.Application.Common.Interfaces;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ResultsEntry.Common;

/// <summary>
/// Resolves the live current range for a simple result. Per Decision 1 the
/// analyte is the only live range source: a simple test mapped to an analyte
/// resolves bands from that analyte's single <see cref="AnalyteReferenceRange"/>
/// aggregate and freezes the matching band into the existing M-04 snapshot.
/// Unmapped (legacy) simple tests keep resolving the historic
/// <see cref="ReferenceRange"/> rows for M-04 compatibility. Mapped tests never
/// read live Test.ReferenceRange.
/// </summary>
internal static class ResultReferenceRangeSource
{
    /// <summary>
    /// Loads the current bands for a simple test's analyte mapping.
    /// Returns <c>null</c> when the test is unmapped (legacy path); otherwise the
    /// analyte's current bands, possibly empty (a mapped test never falls back to
    /// the legacy rows).
    /// </summary>
    public static IReadOnlyList<AnalyteReferenceRangeBand>? LoadAnalyteBands(
        IApplicationDbContext db,
        Test test)
    {
        if (test.AnalyteId is null)
        {
            return null;
        }

        var range = db.Set<AnalyteReferenceRange>().FirstOrDefault(r => r.AnalyteId.Equals(test.AnalyteId));
        if (range is null)
        {
            return Array.Empty<AnalyteReferenceRangeBand>();
        }

        return db.Set<AnalyteReferenceRangeBand>()
            .Where(b => b.AnalyteReferenceRangeId.Equals(range.Id))
            .OrderBy(b => b.AgeMin)
            .ThenBy(b => b.Id.Value)
            .ToList();
    }

    /// <summary>
    /// Captures the frozen historical bounds for the patient into the existing
    /// M-04 snapshot row, from either the analyte band path or the legacy range
    /// path. Returns <c>null</c> when no band/range matches (caller removes any
    /// existing snapshot). Band captures reuse the exact <see cref="ReferenceRangeSnapshot"/>
    /// shape so the persisted snapshot is provider-neutral.
    /// </summary>
    public static PatientTestReferenceRangeSnapshot? Capture(
        PatientTestId patientTestId,
        int testId,
        TopLab.Domain.Common.Enums.Sex sex,
        TopLab.Domain.Common.Enums.AgeUnit ageUnit,
        int ageValue,
        IReadOnlyList<AnalyteReferenceRangeBand>? bands,
        IReadOnlyList<ReferenceRange> ranges)
    {
        if (bands is not null)
        {
            var band = ResultFlagComputer.SelectMatch(sex, ageUnit, ageValue, bands);
            if (band is null)
            {
                return null;
            }

            return PatientTestReferenceRangeSnapshot.FromSnapshot(
                patientTestId,
                new ReferenceRangeSnapshot(
                    testId,
                    band.Sex,
                    band.AgeUnit,
                    band.AgeMin,
                    band.AgeMax,
                    band.MinValue,
                    band.MaxValue,
                    band.LowComment,
                    band.HighComment,
                    System.DateTimeOffset.UtcNow));
        }

        var range = ResultFlagComputer.SelectMatch(sex, ageUnit, ageValue, ranges);
        return range is null ? null : PatientTestReferenceRangeSnapshot.FromSnapshot(patientTestId, range.CaptureSnapshot());
    }
}