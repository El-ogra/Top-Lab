using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.ResultsEntry.Common;

/// <summary>
/// Records the printed state after a print operation has actually succeeded.
/// <para>
/// W-02 post-implementation fix (owner decision 1: <b>printed = successful printing</b>).
/// Before this type existed, printed-state recording was scattered across four call
/// sites with two different conventions: <see cref="PrintCombinedReportCommandHandler"/> and
/// <see cref="PrintHistoryReportCommandHandler"/> marked rows, while the entry screens and
/// bulk print went through <see cref="IResultPrintCoordinator"/>, which cannot persist
/// anything (it takes no <see cref="IApplicationDbContext"/>). The result was two materially
/// different behaviours for the same user action, which silently disabled delivery
/// (<c>MarkDelivered</c> requires <c>IsPrinted</c>) and profile amendment
/// (<c>AmendProfileResultCommandHandler</c> requires <c>ProfileResultItem.IsPrinted</c>).
/// </para>
/// <para>
/// This service is the single place where that state transition happens. It is called
/// <b>only</b> after <c>print.IsSuccess == true</c>. Report <i>building</i> is deliberately
/// NOT its concern — the coordinator and the two report handlers keep building their own
/// payloads exactly as before, so <see cref="IResultPrintCoordinator"/>'s contract
/// ("build, then print, and never mark") stays intact and is not widened here.
/// </para>
/// <para>
/// Layering: Application only. Every port it needs (<see cref="IApplicationDbContext"/>,
/// <see cref="ICurrentUserService"/>, <see cref="IDateTimeProvider"/>) is already declared in
/// <c>Application/Common/Interfaces</c>, so no dependency is introduced and EF access stays
/// behind the existing port.
/// </para>
/// </summary>
public interface IPrintedStateRecorder
{
    /// <summary>
    /// Marks the given patient tests and profile result items as printed and persists them
    /// in a single <c>SaveChangesAsync</c>.
    /// <para>
    /// Nothing is written when both collections are empty, so callers that print a report
    /// carrying no rows do not pay for a no-op save.
    /// </para>
    /// <para>
    /// Fails (owner decision 4, strict) when a domain guard rejects the transition —
    /// currently: a <see cref="PatientTest"/> that is not reviewed, or a
    /// <see cref="ProfileResultItem"/> that is not verified. Domain invariants are never
    /// bypassed here.
    /// </para>
    /// </summary>
    Task<Result> RecordAsync(
        IReadOnlyCollection<int> patientTestIds,
        IReadOnlyCollection<int> profileResultItemIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience overload for the single-test printing paths (entry screens, bulk print),
    /// which know only a patient test id. Resolves that test's profile result items from the
    /// database so the recorded state always matches what a combined/simple report renders.
    /// </summary>
    Task<Result> RecordForPatientTestAsync(
        int patientTestId,
        CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IPrintedStateRecorder"/>
public sealed class PrintedStateRecorder : IPrintedStateRecorder
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public PrintedStateRecorder(
        IApplicationDbContext db,
        ICurrentUserService user,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = user;
        _clock = clock;
    }

    public async Task<Result> RecordAsync(
        IReadOnlyCollection<int> patientTestIds,
        IReadOnlyCollection<int> profileResultItemIds,
        CancellationToken cancellationToken = default)
    {
        if (patientTestIds.Count == 0 && profileResultItemIds.Count == 0)
        {
            return Result.Success();
        }

        var userId = _currentUser.UserId;
        var now = _clock.UtcNow;

        try
        {
            if (patientTestIds.Count > 0)
            {
                var tests = _db.Set<PatientTest>()
                    .Where(pt => patientTestIds.Contains(pt.Id.Value))
                    .ToList();

                foreach (var row in tests)
                {
                    row.MarkPrinted(userId, now);
                }
            }

            if (profileResultItemIds.Count > 0)
            {
                var items = _db.Set<ProfileResultItem>()
                    .Where(i => profileResultItemIds.Contains(i.Id.Value))
                    .ToList();

                foreach (var item in items)
                {
                    item.MarkPrinted(userId, now);
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            // Owner decision 4 (strict): a sheet already left the printer, so the request
            // reports failure honestly rather than silently recording a partial state.
            // Both messages are the ones the per-feature DomainFailureTranslator copies
            // already produce, so no new Arabic string is introduced.
            return Result.Failure(
                Error.Conflict(Translate(ex)));
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RecordForPatientTestAsync(
        int patientTestId,
        CancellationToken cancellationToken = default)
    {
        var itemIds = _db.Set<ProfileResultItem>()
            .Where(i => i.PatientTestId.Value == patientTestId)
            .Select(i => i.Id.Value)
            .ToList();

        return await RecordAsync([patientTestId], itemIds, cancellationToken);
    }

    private static string Translate(InvalidOperationException ex) => ex.Message switch
    {
        "Result not reviewed." => "لا يمكن طباعة نتيجة غير معتمدة.",
        "Profile item not verified." => "المادة غير معتمدة؛ لا يمكن طباعتها.",
        _ => "بيانات غير صالحة."
    };
}