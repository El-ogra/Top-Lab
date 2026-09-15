using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Commands.BuildBlankReport;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.ReportProduction.Commands.PrintBlankReport;

public sealed class PrintBlankReportCommandHandler : IRequestHandler<PrintBlankReportCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ISender _sender;
    private readonly IReportPrintingService _printing;

    public PrintBlankReportCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ISender sender,
        IReportPrintingService printing)
    {
        _db = db;
        _currentUser = currentUser;
        _sender = sender;
        _printing = printing;
    }

    public async Task<Result> Handle(PrintBlankReportCommand request, CancellationToken cancellationToken)
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

        var built = await _sender.Send(new BuildBlankReportCommand(request.PatientId), cancellationToken);
        if (!built.IsSuccess)
        {
            return Result.Failure(built.Error!);
        }

        // Blank report carries patient data only — no result lines, so no BR-07
        // balance gate and no MarkPrinted audit (OD-07-E).
        var token = ReportPrintEnvelope.CreateToken(ReportPrintEnvelope.Blank, built.Value!);
        var print = await _printing.PrintReportAsync(token, cancellationToken);
        return print;
    }
}