using TopLab.Application.Common.Interfaces;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Patients;
using TopLab.Domain.Reports;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ReportProduction.Common;

/// <summary>
/// Shared history reader: resolves every patient sharing the target patient's
/// identity (per <see cref="HistorySortMode"/>) and assembles the history entry DTOs.
/// Auto/manual history insertion (S4) reuses these two helpers — no rollup logic is
/// duplicated outside this type.
/// </summary>
internal static class PatientHistoryReader
{
    internal static IReadOnlyList<Patient> ResolveVisitPatients(
        IApplicationDbContext db,
        Patient patient,
        ReportSettings settings)
    {
        if (settings.HistorySortMode == HistorySortMode.ByPatientName)
        {
            var key = PatientHistoryResolver.ResolveKey(
                HistorySortMode.ByPatientName, null, patient.FullName);

            // W-02 S12/S15 (WP-10/WP-29): never materialise the whole Patient table.
            // Pull a bounded candidate set in SQL (trimmed upper-cased names starting
            // with the key's first token — all translatable: TRIM/UPPER/LIKE), then apply
            // the exact normalised comparison in memory. NOTE: the SQL pre-filter uses
            // ToUpper() (the only translatable fold) while the exact match uses
            // ToUpperInvariant(); under exotic collations/cultures a candidate could be
            // missed by the pre-filter — the exact comparison, not the pre-filter,
            // defines membership, and any miss surfaces as a missing visit, never as
            // another patient's data.
            var firstToken = key.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? key;
            return db.Set<Patient>()
                .Where(p => !p.IsDeleted && p.FullName != null && p.FullName.Trim().ToUpper().StartsWith(firstToken))
                .ToList()
                .Where(p => !string.IsNullOrWhiteSpace(p.FullName)
                    && PatientHistoryResolver.ResolveKey(
                        HistorySortMode.ByPatientName, null, p.FullName) == key)
                .OrderBy(p => p.FullName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        var labKey = PatientHistoryResolver.ResolveKey(
            HistorySortMode.ByLabCode, patient.LabId?.Value, null!);

        return db.Set<Patient>()
            .Where(p => !p.IsDeleted && p.LabId != null && p.LabId.Value.Trim() == labKey)
            .OrderBy(p => p.LabId!.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    internal static IReadOnlyList<HistoryEntryDto> BuildEntries(
        IApplicationDbContext db,
        IReadOnlyList<Patient> patients,
        DateOnly? from = null,
        DateOnly? to = null,
        int? testId = null)
    {
        if (patients.Count == 0)
        {
            return Array.Empty<HistoryEntryDto>();
        }

        var fromStart = from?.ToDateTime(TimeOnly.MinValue);
        var toEnd = to?.ToDateTime(TimeOnly.MaxValue);

        var patientsById = patients.ToDictionary(p => p.Id.Value);
        var catalog = db.Set<Test>().ToDictionary(t => t.Id.Value);
        var rows = db.Set<PatientTest>()
            .Where(pt => patientsById.Keys.Contains(pt.PatientId.Value)
                && (!testId.HasValue || pt.TestId.Value == testId.Value)
                && (!fromStart.HasValue || (pt.EnteredAtUtc.HasValue && pt.EnteredAtUtc.Value >= fromStart.Value))
                && (!toEnd.HasValue || (pt.EnteredAtUtc.HasValue && pt.EnteredAtUtc.Value <= toEnd.Value)))
            .ToList();

        var rowsByPatient = rows
            .GroupBy(pt => pt.PatientId.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var testIds = rows.Select(r => r.Id.Value).ToList();
        var snapshots = testIds.Count == 0
            ? new Dictionary<int, PatientTestReferenceRangeSnapshot>()
            : db.Set<PatientTestReferenceRangeSnapshot>()
                .Where(s => testIds.Contains(s.PatientTestId.Value))
                .ToList()
                .GroupBy(s => s.PatientTestId.Value)
                .ToDictionary(g => g.Key, g => g.First());

        // W-02 S8 (WP-13): one aggregated read of test comments — no per-row query.
        var catalogTestIds = rows.Select(r => r.TestId.Value).Distinct().ToList();
        var commentsByTest = catalogTestIds.Count == 0
            ? new Dictionary<int, IReadOnlyList<string>>()
            : db.Set<TestComment>()
                .Where(c => catalogTestIds.Contains(c.TestId.Value))
                .OrderBy(c => c.Id.Value)
                .ToList()
                .GroupBy(c => c.TestId.Value)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<string>)g.Select(c => c.CommentText).ToList());

        IReadOnlyList<HistoryEntryDto> entries = patientsById.Keys
            .SelectMany(id => rowsByPatient.TryGetValue(id, out var list)
                ? list.OrderByDescending(pt => pt.EnteredAtUtc).ThenByDescending(pt => pt.Id.Value)
                : Enumerable.Empty<PatientTest>())
            .Select(pt =>
            {
                catalog.TryGetValue(pt.TestId.Value, out var test);
                snapshots.TryGetValue(pt.Id.Value, out var snap);
                var (lowComment, highComment) = RangeComments(snap, pt.ResultFlag);
                commentsByTest.TryGetValue(pt.TestId.Value, out var testComments);
                return new HistoryEntryDto(
                    pt.Id.Value,
                    pt.PatientId.Value,
                    pt.TestId.Value,
                    test?.Name ?? string.Empty,
                    test?.TestCode ?? string.Empty,
                    test == null ? 0 : (int)test.ResultKind,
                    pt.ResultValue,
                    pt.ResultFlag == null ? null : (int)pt.ResultFlag.Value,
                    pt.IsReviewed,
                    pt.EnteredAtUtc,
                    pt.ReviewedAtUtc,
                    lowComment,
                    highComment,
                    pt.IsTakenOutsideLab,
                    testComments,
                    pt.EnteredAtUtc is null ? null : DateOnly.FromDateTime(pt.EnteredAtUtc.Value));
            })
            .ToList();

        return entries;
    }

    /// <summary>
    /// WP-07: comments come from the frozen snapshot (never the live range) and
    /// only on the matching out-of-range flag.
    /// </summary>
    internal static (string? Low, string? High) RangeComments(
        PatientTestReferenceRangeSnapshot? frozen,
        ResultFlag? flag)
    {
        if (frozen is null)
        {
            return (null, null);
        }

        return flag switch
        {
            ResultFlag.Low => (frozen.LowComment, null),
            ResultFlag.High => (null, frozen.HighComment),
            _ => (null, null)
        };
    }

    internal static IReadOnlyList<Patient> OrderByIdentity(
        IReadOnlyList<Patient> patients,
        HistorySortMode mode)
    {
        return mode == HistorySortMode.ByPatientName
            ? patients.OrderBy(p => p.FullName, StringComparer.OrdinalIgnoreCase).ToList()
            : patients.OrderBy(p => p.LabId?.Value, StringComparer.OrdinalIgnoreCase).ToList();
    }
}