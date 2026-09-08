using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.ResultsEntry.Commands.UnreviewResult;

public sealed class UnreviewResultCommandHandler : IRequestHandler<UnreviewResultCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UnreviewResultCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UnreviewResultCommand request, CancellationToken cancellationToken)
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

        if (!pt.IsReviewed)
        {
            return Result.Success();
        }

        try
        {
            pt.Unreview();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Conflict(DomainFailureTranslator.Translate(ex)));
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
