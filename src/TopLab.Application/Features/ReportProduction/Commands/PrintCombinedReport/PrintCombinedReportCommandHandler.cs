using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport;
using TopLab.Application.Features.ReportProduction.Common;
using BalanceProbe = TopLab.Application.Features.ResultsEntry.Common.BalanceProbe;
using IPrintedStateRecorder = TopLab.Application.Features.ResultsEntry.Common.IPrintedStateRecorder;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.ReportProduction.Commands.PrintCombinedReport;

public sealed class PrintCombinedReportCommandHandler : IRequestHandler<PrintCombinedReportCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ISender _sender;
    private readonly IReportPrintingService _printing;
    private readonly IPrintedStateRecorder _printedState;

    public PrintCombinedReportCommandHandler(
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

    public async Task<Result> Handle(PrintCombinedReportCommand request, CancellationToken cancellationToken)
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

        var built = await _sender.Send(
            new BuildCombinedReportCommand(request.PatientId, request.OrderedPatientTestIds),
            cancellationToken);
        if (!built.IsSuccess)
        {
            return Result.Failure(built.Error!);
        }

        if (user.BlockPrintOnRemainingBalance
            && !_currentUser.IsAbsolutePermission
            && BalanceProbe.Balance(_db, patient.Id.Value) > 0)
        {
            return Result.Failure(Error.Conflict("يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة."));
        }

        var token = ReportPrintEnvelope.CreateToken(ReportPrintEnvelope.Combined, built.Value!);
        var print = await _printing.PrintReportAsync(token, cancellationToken);
        if (!print.IsSuccess)
        {
            return print;
        }

        var ids = built.Value!.Lines.Select(l => l.PatientTestId).ToList();
        var profileItemIds = built.Value!.Lines
            .SelectMany(l => l.ProfileLines)
            .Select(p => p.ProfileResultItemId)
            .Where(id => id != 0)
            .Distinct()
            .ToList();

        // W-02 post-implementation fix (owner decision 1: printed = successful printing).
        // Printed state is recorded in exactly one place, through IPrintedStateRecorder,
        // so the combined report records the same state as every other printing path.
        // It also now records ProfileResultItem.IsPrinted for the profile lines carried
        // by this report, which is what restores amendment after printing (decision 2).
        // The balance gate, the "print failed" early return, and the translated domain
        // error contract above/below are unchanged.
        var recorded = await _printedState.RecordAsync(ids, profileItemIds, cancellationToken);
        if (!recorded.IsSuccess)
        {
            return recorded;
        }

        return Result.Success();
    }
}