using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.ResultsEntry.Commands.ClearResult;

public sealed class ClearResultCommandHandler : IRequestHandler<ClearResultCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public ClearResultCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(ClearResultCommand request, CancellationToken cancellationToken)
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

        try
        {
            pt.ClearResult();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Conflict(DomainFailureTranslator.Translate(ex)));
        }

        var existing = _db.Set<PatientTestReferenceRangeSnapshot>()
            .FirstOrDefault(s => s.PatientTestId.Value == pt.Id.Value);
        if (existing is not null)
        {
            _db.Remove(existing);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
