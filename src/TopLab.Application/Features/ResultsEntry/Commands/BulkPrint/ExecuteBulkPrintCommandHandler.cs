using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;

public sealed class ExecuteBulkPrintCommandHandler
    : IRequestHandler<ExecuteBulkPrintCommand, Result<IReadOnlyList<BulkPrintOutcomeDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IResultPrintCoordinator _printCoordinator;

    public ExecuteBulkPrintCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IResultPrintCoordinator printCoordinator)
    {
        _db = db;
        _currentUser = currentUser;
        _printCoordinator = printCoordinator;
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

            var requiresConfirmation = verified.Any(pt => pt.IsPrinted);
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
