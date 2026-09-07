using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.PatientRegistration.Commands.UpdatePatientTestSampleFlags;

public sealed class UpdatePatientTestSampleFlagsCommandHandler : IRequestHandler<UpdatePatientTestSampleFlagsCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;

    public UpdatePatientTestSampleFlagsCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<bool>> Handle(UpdatePatientTestSampleFlagsCommand request, CancellationToken cancellationToken)
    {
        var pt = _db.Set<PatientTest>().FirstOrDefault(t => t.Id.Value == request.PatientTestId);
        if (pt is null)
        {
            return Result<bool>.Failure(Error.NotFound("التحليل غير موجود."));
        }

        pt.UpdateSampleFlags(
            request.IsUrine,
            request.IsStool,
            request.IsBlood,
            request.IsSemen,
            request.IsCsf,
            request.IsTakenOutsideLab);

        await _db.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}