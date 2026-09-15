using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ReportProduction.Queries.GetSeparateHistoryReport;
using BalanceProbe = TopLab.Application.Features.ResultsEntry.Common.BalanceProbe;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.ReportProduction.Commands.PrintHistoryReport;

public sealed class PrintHistoryReportCommandHandler : IRequestHandler<PrintHistoryReportCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly ISender _sender;
    private readonly IReportPrintingService _printing;

    public PrintHistoryReportCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock,
        ISender sender,
        IReportPrintingService printing)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _sender = sender;
        _printing = printing;
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
        var rows = _db.Set<PatientTest>().Where(pt => ids.Contains(pt.Id.Value)).ToList();
        foreach (var row in rows)
        {
            try
            {
                row.MarkPrinted(_currentUser.UserId, _clock.UtcNow);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(Error.Conflict(DomainFailureTranslator.Translate(ex)));
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}