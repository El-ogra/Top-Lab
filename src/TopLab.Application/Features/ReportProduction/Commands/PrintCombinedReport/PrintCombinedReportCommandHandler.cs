using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport;
using TopLab.Application.Features.ReportProduction.Common;
using BalanceProbe = TopLab.Application.Features.ResultsEntry.Common.BalanceProbe;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.ReportProduction.Commands.PrintCombinedReport;

public sealed class PrintCombinedReportCommandHandler : IRequestHandler<PrintCombinedReportCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly ISender _sender;
    private readonly IReportPrintingService _printing;

    public PrintCombinedReportCommandHandler(
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