using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.RemoveProfileAnalyte;

public sealed class RemoveProfileAnalyteCommandHandler : IRequestHandler<RemoveProfileAnalyteCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public RemoveProfileAnalyteCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(RemoveProfileAnalyteCommand request, CancellationToken cancellationToken)
    {
        var profile = _db.Set<Profile>().FirstOrDefault(p => p.Id.Value == request.ProfileId);
        if (profile is null)
        {
            return Result.Failure(Error.NotFound("البروفايل غير موجود."));
        }

        var analyte = _db.Set<Analyte>().FirstOrDefault(a => a.Id.Value == request.AnalyteId);
        if (analyte is null)
        {
            return Result.Failure(Error.NotFound("المادة التحليلية غير موجودة"));
        }

        if (_db.Set<PatientTest>().Any(pt => pt.TestId.Equals(profile.TestId) && pt.EnteredAtUtc != null))
        {
            return Result.Failure(Error.Conflict("لا يمكن تعديل مكونات البروفايل بعد إدخال النتائج."));
        }

        var link = _db.Set<ProfileAnalyte>()
            .FirstOrDefault(l => l.ProfileId.Equals(profile.Id) && l.AnalyteId.Equals(analyte.Id));
        if (link is null)
        {
            return Result.Failure(Error.Conflict("المادة التحليلية غير مرتبطة بالبروفايل."));
        }

        try
        {
            profile.RemoveAnalyte(analyte.Id);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(Error.Conflict("المادة التحليلية غير مرتبطة بالبروفايل."));
        }

        _db.Remove(link);

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}