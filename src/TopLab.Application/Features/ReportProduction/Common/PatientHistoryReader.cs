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

            return db.Set<Patient>()
                .Where(p => !p.IsDeleted)
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
        IReadOnlyList<Patient> patients)
    {
        if (patients.Count == 0)
        {
            return Array.Empty<HistoryEntryDto>();
        }

        var patientsById = patients.ToDictionary(p => p.Id.Value);
        var catalog = db.Set<Test>().ToDictionary(t => t.Id.Value);
        var rows = db.Set<PatientTest>()
            .Where(pt => patientsById.Keys.Contains(pt.PatientId.Value))
            .ToList();

        var rowsByPatient = rows
            .GroupBy(pt => pt.PatientId.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        IReadOnlyList<HistoryEntryDto> entries = patientsById.Keys
            .SelectMany(id => rowsByPatient.TryGetValue(id, out var list)
                ? list.OrderByDescending(pt => pt.EnteredAtUtc).ThenByDescending(pt => pt.Id.Value)
                : Enumerable.Empty<PatientTest>())
            .Select(pt =>
            {
                catalog.TryGetValue(pt.TestId.Value, out var test);
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
                    pt.ReviewedAtUtc);
            })
            .ToList();

        return entries;
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