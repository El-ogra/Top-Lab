using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ReportProduction.Queries.GetSeparateHistoryReport;
using BalanceProbe = TopLab.Application.Features.ResultsEntry.Common.BalanceProbe;
using IPrintedStateRecorder = TopLab.Application.Features.ResultsEntry.Common.IPrintedStateRecorder;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.ReportProduction.Commands.PrintHistoryReport;

public sealed class PrintHistoryReportCommandHandler : IRequestHandler<PrintHistoryReportCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ISender _sender;
    private readonly IReportPrintingService _printing;
    private readonly IPrintedStateRecorder _printedState;

    public PrintHistoryReportCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock,
        ISender sender,
        IReportPrintingService printing,
        IPrintedStateRecorder printedState)
    {
        _db = db;
        _currentUser = currentUser;
        _sender = sender;
        _printing = printing;
        _printedState = printedState;
    }

    public async Task<Result> Handle(PrintHistoryReportCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Result.Failure(Error.NotFound("المريض غير موجود."));
        }

        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == _currentUser.UserId);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("المستخدم غير موجود."));
        }

        var history = await _sender.Send(new GetSeparateHistoryReportQuery(request.PatientId), cancellationToken);
        if (!history.IsSuccess)
        {
            return Result.Failure(history.Error!);
        }

        // The standalone history report is reviewed-only (FR-M07-007); nothing
        // eligible means nothing printable.
        if (!history.Value!.Entries.Any(e => e.IsReviewed))
        {
            return Result.Failure(Error.Conflict("لا يمكن طباعة نتيجة غير معتمدة."));
        }

        if (user.BlockPrintOnRemainingBalance
            && !_currentUser.IsAbsolutePermission
            && BalanceProbe.Balance(_db, patient.Id.Value) > 0)
        {
            return Result.Failure(Error.Conflict("يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة."));
        }

        var token = ReportPrintEnvelope.CreateToken(ReportPrintEnvelope.History, history.Value!);
        var print = await _printing.PrintReportAsync(token, cancellationToken);
        if (!print.IsSuccess)
        {
            return print;
        }

        var ids = history.Value!.Entries.Where(e => e.IsReviewed).Select(e => e.PatientTestId).ToList();

        // W-02 post-implementation fix (owner decision 1: printed = successful printing).
        // Printed state is recorded in exactly one place, through IPrintedStateRecorder.
        // No profile item ids are passed: the history report payload (HistoryEntryDto)
        // never carries profile lines — PatientHistoryReader does not read
        // ProfileResultItem — so there is nothing to record at item level here.
        var recorded = await _printedState.RecordAsync(ids, Array.Empty<int>(), cancellationToken);
        if (!recorded.IsSuccess)
        {
            return recorded;
        }

        return Result.Success();
    }
}