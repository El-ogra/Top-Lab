using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.WorkSheets.Common;

/// <summary>
/// Owner-settled period semantics shared by all four M-11 queries: both bounds
/// default to the current day in UTC when unspecified; <c>To ??= From</c>;
/// bounds are inclusive on UTC calendar days.
/// </summary>
internal static class WorkSheetPeriod
{
    public static (DateOnly From, DateOnly To) Resolve(DateOnly? from, DateOnly? to)
    {
        var resolvedFrom = from ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var resolvedTo = to ?? resolvedFrom;
        return (resolvedFrom, resolvedTo);
    }

    public static bool IsValid(DateOnly? from, DateOnly? to)
    {
        var (resolvedFrom, resolvedTo) = Resolve(from, to);
        return resolvedFrom <= resolvedTo;
    }
}

/// <summary>
/// Shared bench-sheet row selector: <see cref="PatientTest"/> rows for the period
/// whose sample should be in the lab (<c>!IsTakenOutsideLab</c>), ordered by
/// patient registration then line id. Each line carries the owner-settled
/// scannable identity pair (<c>PatientTestId</c> + <c>LabId</c>); <c>Barcode</c>
/// is the test classifier from the catalog.
/// </summary>
internal static class WorkSheetLines
{
    public static IReadOnlyList<WorkSheetLineDto> Select(
        IApplicationDbContext db,
        HashSet<int> testIds,
        DateOnly from,
        DateOnly to)
    {
        var patients = db.Set<Patient>()
            .Where(p => !p.IsDeleted
                && DateOnly.FromDateTime(p.RegistrationDateUtc) >= from
                && DateOnly.FromDateTime(p.RegistrationDateUtc) <= to)
            .ToDictionary(p => p.Id.Value);

        var tests = db.Set<Test>().ToDictionary(t => t.Id.Value);

        return db.Set<PatientTest>()
            .Where(pt => testIds.Contains(pt.TestId.Value) && !pt.IsTakenOutsideLab)
            .ToList()
            .Where(pt => patients.ContainsKey(pt.PatientId.Value))
            .OrderBy(pt => patients[pt.PatientId.Value].RegistrationDateUtc)
            .ThenBy(pt => pt.Id.Value)
            .Select(pt =>
            {
                var patient = patients[pt.PatientId.Value];
                tests.TryGetValue(pt.TestId.Value, out var test);
                return new WorkSheetLineDto(
                    pt.Id.Value,
                    patient.Id.Value,
                    patient.FullName,
                    patient.LabId == null ? null : patient.LabId.Value,
                    test?.Name ?? string.Empty,
                    test?.TestCode ?? string.Empty,
                    test?.Barcode,
                    pt.IsSampleDrawn,
                    pt.SampleDrawnAtUtc,
                    pt.EnteredAtUtc != null,
                    pt.IsReviewed,
                    test?.CompletionDurationMinutes ?? 0);
            })
            .ToList();
    }
}
