using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.AddProfileAnalyte;

public sealed class AddProfileAnalyteCommandHandler : IRequestHandler<AddProfileAnalyteCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public AddProfileAnalyteCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(AddProfileAnalyteCommand request, CancellationToken cancellationToken)
    {
        var profile = _db.Set<Profile>().FirstOrDefault(p => p.Id.Value == request.ProfileId);
        if (profile is null)
        {
            return Result.Failure(Error.NotFound("البروفايل غير موجود."));
        }

        var analyte = _db.Set<Analyte>().FirstOrDefault(a => a.Id.Value == request.AnalyteId);
        if (analyte is null || !analyte.IsActive)
        {
            return Result.Failure(Error.NotFound("المادة التحليلية غير موجودة"));
        }

        if (_db.Set<PatientTest>().Any(pt => pt.TestId.Equals(profile.TestId) && pt.EnteredAtUtc != null))
        {
            return Result.Failure(Error.Conflict("لا يمكن تعديل مكونات البروفايل بعد إدخال النتائج."));
        }

        var link = ProfileAnalyte.Create(ProfileAnalyteId.Create(0), profile.Id, analyte.Id);

        try
        {
            profile.AddAnalyte(link);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(Error.Conflict("المادة التحليلية مرتبطة بالبروفايل بالفعل."));
        }

        _db.Add(link);

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}