using MediatR;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IResultPrintCoordinator _coordinator;

    /// <summary>
    /// W-02 S5 / SD-16: the coordinator is required, so there is exactly one construction path and
    /// MediatR's DI resolution stays unambiguous. (An earlier revision added a clock overload to
    /// keep the C-26 tests compiling; it turned out those files reference only the *command*, so
    /// the overload was removed in S7 and only ReviewPrintDeliver needed a one-line swap.)
    /// </summary>
    public MarkResultPrintedCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IResultPrintCoordinator coordinator)
    {
        _db = db;
        _currentUser = currentUser;
        _coordinator = coordinator;
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
        var outcome = await _coordinator.PrintAsync(
            request.PatientTestId, ResultPrintKind.SimpleResult, cancellationToken);

        return outcome.Printed
            ? Result.Success()
            : Result.Failure(Error.Conflict(outcome.ErrorMessage ?? "تعذّرت الطباعة."));
    }
}
