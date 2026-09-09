using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.CreateProfile;

public sealed class CreateProfileCommandHandler : IRequestHandler<CreateProfileCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public CreateProfileCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(CreateProfileCommand request, CancellationToken cancellationToken)
    {
        var test = _db.Set<Test>().FirstOrDefault(t => t.Id.Value == request.SpecializedTestId);
        if (test is null)
        {
            return Result<int>.Failure(Error.NotFound("التحليل غير موجود"));
        }

        if (test.ResultKind != ResultKind.SpecializedProfile)
        {
            return Result<int>.Failure(Error.Conflict("البروفايل يُنشأ فقط لتحليل متخصص."));
        }

        if (_db.Set<Profile>().Any(p => p.TestId.Equals(test.Id)))
        {
            return Result<int>.Failure(Error.Conflict("هذا التحليل له بروفايل بالفعل."));
        }

        var ids = (request.AnalyteIds ?? Array.Empty<int>()).Distinct().ToList();
        foreach (var analyteId in ids)
        {
            var analyte = _db.Set<Analyte>().FirstOrDefault(a => a.Id.Value == analyteId);
            if (analyte is null || !analyte.IsActive)
            {
                return Result<int>.Failure(Error.NotFound("المادة التحليلية غير موجودة"));
            }

            if (!_db.Set<AnalyteReferenceRange>().Any(r => r.AnalyteId.Equals(analyte.Id)))
            {
                return Result<int>.Failure(Error.Conflict("يجب تحديد نطاق مرجعي للمادة التحليلية قبل إضافتها للبروفايل."));
            }
        }

        Profile profile;
        try
        {
            profile = Profile.Create(ProfileId.Create(0), request.Name?.Trim() ?? string.Empty, test.Id, test.ResultKind, request.FixedPrice);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(Error.Validation(ex.Message));
        }

        _db.Add(profile);

        foreach (var analyteId in ids)
        {
            var link = ProfileAnalyte.Create(ProfileAnalyteId.Create(0), profile.Id, AnalyteId.Create(analyteId));
            profile.AddAnalyte(link);
            _db.Add(link);
        }

        await _db.SaveChangesAsync(cancellationToken);

        var created = _db.Set<Profile>().FirstOrDefault(p => p.Id.Value == profile.Id.Value);
        return created is null
            ? Result<int>.Failure(Error.Unexpected("تعذّر إنشاء البروفايل."))
            : Result<int>.Success(created.Id.Value);
    }
}