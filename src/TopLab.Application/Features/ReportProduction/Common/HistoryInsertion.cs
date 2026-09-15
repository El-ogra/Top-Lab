namespace TopLab.Application.Features.ReportProduction.Common;

/// <summary>
/// Persistence-free history insertion mapper (S4): turns a history entry into a
/// combined-report model line. Insertion only copies values into the DTO model —
/// no stored <see cref="TopLab.Domain.Results.PatientTest"/> row is ever mutated
/// (pinned by the no-mutation test in both S4 handler test classes).
/// </summary>
internal static class HistoryInsertion
{
    internal static CombinedReportLineDto LineFromEntry(HistoryEntryDto entry)
    {
        return new CombinedReportLineDto(
            entry.PatientTestId,
            entry.TestId,
            entry.TestName,
            entry.TestCode,
            entry.ResultKind,
            entry.ResultValue,
            entry.ResultFlag,
            FrozenRangeText: null,
            ProfileLines: Array.Empty<ProfileReportLineDto>(),
            Culture: null);
    }
}