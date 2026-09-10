using TopLab.Application.Common.Interfaces;
using TopLab.Domain.PatientStatus;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.PatientSearch.Common;

/// <summary>
/// Per-visit rollup shared by the LabId lookup and the visit-history query.
/// The aggregate status is the settled seven-state calculator over each visit's own
/// tests and balance (FR-M08-007, PRD §8); All* booleans are true when TestCount &gt; 0
/// and the respective count equals TestCount (an empty visit reports all-false).
/// </summary>
internal static class VisitRollup
{
    public static VisitSummaryDto Summarize(
        IApplicationDbContext db,
        Patient patient,
        IReadOnlyList<PatientTest> tests)
    {
        var balance = BalanceProbe.Balance(db, patient.Id.Value);

        var testCount = tests.Count;
        var resultsEntered = tests.Count(pt => pt.EnteredAtUtc is not null);
        var reviewed = tests.Count(pt => pt.IsReviewed);
        var printed = tests.Count(pt => pt.IsPrinted);
        var delivered = tests.Count(pt => pt.IsDelivered);

        var status = (int)new PatientStatusCalculator().Calculate(patient, tests, balance);

        return new VisitSummaryDto(
            patient.Id.Value,
            patient.LabId?.Value,
            patient.RegistrationDateUtc,
            patient.AccountType.ToString(),
            patient.IsVip,
            testCount,
            resultsEntered,
            reviewed,
            printed,
            delivered,
            testCount > 0 && resultsEntered == testCount,
            testCount > 0 && reviewed == testCount,
            testCount > 0 && printed == testCount,
            testCount > 0 && delivered == testCount,
            status,
            balance);
    }

    public static int AggregateStatus(
        IApplicationDbContext db,
        Patient patient,
        IReadOnlyList<PatientTest> tests)
    {
        var balance = BalanceProbe.Balance(db, patient.Id.Value);
        return (int)new PatientStatusCalculator().Calculate(patient, tests, balance);
    }

    public static VisitHistoryDto BuildHistory(
        string labId,
        string patientFullName,
        ReportSettings settings,
        IReadOnlyList<Patient> visits,
        IReadOnlyList<PatientTest> tests,
        IApplicationDbContext db)
    {
        var testsByPatient = tests
            .GroupBy(pt => pt.PatientId.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<PatientTest>)g.ToList());

        var summaries = visits
            .OrderByDescending(v => v.RegistrationDateUtc)
            .Select(v => Summarize(
                db,
                v,
                testsByPatient.TryGetValue(v.Id.Value, out var visitTests)
                    ? visitTests
                    : System.Array.Empty<PatientTest>()))
            .ToList();

        return new VisitHistoryDto(
            labId,
            patientFullName,
            settings.HistorySortMode.ToString(),
            settings.HistoryAutoDisplayEnabled,
            summaries);
    }
}