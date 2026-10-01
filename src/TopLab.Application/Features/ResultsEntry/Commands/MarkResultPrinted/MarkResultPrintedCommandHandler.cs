using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.ResultsEntry.Commands.MarkResultPrinted;

public sealed class MarkResultPrintedCommandHandler : IRequestHandler<MarkResultPrintedCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IResultPrintCoordinator? _coordinator;

    /// <summary>
    /// W-02 S5 / SD-16: the coordinator is an optional trailing parameter so the four C-26 test
    /// files keep compiling untouched (SD-16 forbids editing them). Production always receives a
    /// coordinator — it is registered in <c>AddApplication</c>. A missing coordinator is treated
    /// as "cannot honestly print" rather than falling back to marking the row.
    /// </summary>
    public MarkResultPrintedCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IResultPrintCoordinator? coordinator = null)
    {
        _db = db;
        _currentUser = currentUser;
        _coordinator = coordinator;
    }

    /// <summary>
    /// Retained so the C-26 test files that pass a clock as the third argument keep compiling
    /// without edits. The clock is no longer needed because the handler no longer marks the row.
    /// </summary>
    public MarkResultPrintedCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
        : this(db, currentUser, coordinator: null)
    {
    }

    public async Task<Result> Handle(MarkResultPrintedCommand request, CancellationToken cancellationToken)
    {
        var pt = _db.Set<PatientTest>().FirstOrDefault(t => t.Id.Value == request.PatientTestId);
        if (pt is null)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == pt.PatientId.Value);
        if (patient is null || patient.IsDeleted)
        {
            return Result.Failure(Error.NotFound("المريض غير موجود."));
        }

        if (pt.EnteredAtUtc is null || !pt.IsReviewed)
        {
            return Result.Failure(Error.Conflict("لا يمكن طباعة نتيجة غير معتمدة."));
        }

        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == _currentUser.UserId);
        if (user is not null
            && user.BlockPrintOnRemainingBalance
            && !_currentUser.IsAbsolutePermission
            && BalanceProbe.Balance(_db, patient.Id.Value) > 0)
        {
            return Result.Failure(Error.Conflict("يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة."));
        }

        // W-02 S5 / SD-16 (C-21 + C-26): the binding default is WIRE, not delete. The command and
        // its four test files stay; this handler simply stops lying — it goes through the honest
        // coordinator instead of marking the row. The guards above are unchanged, so no error
        // contract moves.
        if (_coordinator is null)
        {
            return Result.Failure(Error.Conflict("تعذّرت الطباعة: خدمة الطباعة غير متاحة."));
        }

        var outcome = await _coordinator.PrintAsync(
            request.PatientTestId, ResultPrintKind.SimpleResult, cancellationToken);

        return outcome.Printed
            ? Result.Success()
            : Result.Failure(Error.Conflict(outcome.ErrorMessage ?? "تعذّرت الطباعة."));
    }
}
