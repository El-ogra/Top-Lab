using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;

public sealed class ExecuteBulkPrintCommandHandler
    : IRequestHandler<ExecuteBulkPrintCommand, Result<IReadOnlyList<BulkPrintOutcomeDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IResultPrintCoordinator _printCoordinator;
    private readonly IPrintedStateRecorder _printedState;

    public ExecuteBulkPrintCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IResultPrintCoordinator printCoordinator,
        IPrintedStateRecorder printedState)
    {
        _db = db;
        _currentUser = currentUser;
        _printCoordinator = printCoordinator;
        _printedState = printedState;
    }

    public async Task<Result<IReadOnlyList<BulkPrintOutcomeDto>>> Handle(
        ExecuteBulkPrintCommand request, CancellationToken cancellationToken)
    {
        var outcomes = new List<BulkPrintOutcomeDto>();

        foreach (var decision in request.Decisions)
        {
            var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == decision.PatientId);
            if (patient is null || patient.IsDeleted)
            {
                outcomes.Add(new BulkPrintOutcomeDto(decision.PatientId, BulkPrintOutcomes.PatientNotFound, 0));
                continue;
            }

            var verified = _db.Set<PatientTest>()
                .Where(pt => pt.PatientId.Value == patient.Id.Value
                    && pt.EnteredAtUtc != null
                    && pt.IsReviewed)
                .ToList();

            if (verified.Count == 0)
            {
                outcomes.Add(new BulkPrintOutcomeDto(patient.Id.Value, BulkPrintOutcomes.NoVerifiedResults, 0));
                continue;
            }

            // W-02 S8 (WP-13): the reprint prompt is suppressed when the lab configured so.
            var suppressReprint = _db.Set<ReportSettings>()
                .Where(s => s.Id == 1)
                .Select(s => s.SuppressReprintMessage)
                .FirstOrDefault();
            var requiresConfirmation = !suppressReprint && verified.Any(pt => pt.IsPrinted);
            if (requiresConfirmation && !decision.ConfirmReprint)
            {
                outcomes.Add(new BulkPrintOutcomeDto(patient.Id.Value, BulkPrintOutcomes.Skipped, 0));
                continue;
            }

            var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == _currentUser.UserId);
            if (user is not null
                && user.BlockPrintOnRemainingBalance
                && !_currentUser.IsAbsolutePermission
                && BalanceProbe.Balance(_db, patient.Id.Value) > 0)
            {
                outcomes.Add(new BulkPrintOutcomeDto(patient.Id.Value, BulkPrintOutcomes.BlockedByBalance, 0));
                continue;
            }

            // W-02 S6 (WP-06): build then print, never mark (SD-1). One failure does not abort
            // the batch — the remaining tests for this patient, and every later patient, continue.
            var printed = 0;
            string? failure = null;
            foreach (var pt in verified)
            {
                var outcome = await _printCoordinator.PrintAsync(
                    pt.Id.Value, ResultPrintKind.SimpleResult, cancellationToken);

                if (outcome.Printed)
                {
                    // W-02 post-implementation fix (owner decision 1: printed = successful
                    // printing). Bulk print now records printed state too, through the same
                    // single service every other printing path uses. Recorded only for the
                    // sheets that actually printed — a failure leaves the row untouched.
                    // This is what makes the reprint prompt (requiresConfirmation, which
                    // reads IsPrinted) work again after the fix.
                    // Profile items are resolved by the service from the patient test id,
                    // so the printed sheet and the recorded items always agree.
                    var recorded = await _printedState.RecordForPatientTestAsync(
                        pt.Id.Value, cancellationToken);
                    if (!recorded.IsSuccess)
                    {
                        if (failure is null)
                        {
                            failure = recorded.Error!.Message;
                        }

                        continue;
                    }

                    printed++;
                }
                else if (failure is null)
                {
                    failure = outcome.ErrorMessage;
                }
            }

            // Printed only when every verified test produced a sheet; otherwise report Failed.
            outcomes.Add(failure is null
                ? new BulkPrintOutcomeDto(patient.Id.Value, BulkPrintOutcomes.Printed, printed)
                : new BulkPrintOutcomeDto(patient.Id.Value, BulkPrintOutcomes.Failed, printed));
        }

        return Result<IReadOnlyList<BulkPrintOutcomeDto>>.Success(outcomes);
    }
}
