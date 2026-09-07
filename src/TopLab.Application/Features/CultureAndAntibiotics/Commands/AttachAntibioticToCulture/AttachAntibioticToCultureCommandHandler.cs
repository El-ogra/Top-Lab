using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.CultureAndAntibiotics.Commands.AttachAntibioticToCulture;

public sealed class AttachAntibioticToCultureCommandHandler
    : IRequestHandler<AttachAntibioticToCultureCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public AttachAntibioticToCultureCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(
        AttachAntibioticToCultureCommand request, CancellationToken cancellationToken)
    {
        var test = _db.Set<Test>().FirstOrDefault(t => t.Id.Value == request.TestId);
        if (test is null)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        if (!test.IsCultureType)
        {
            return Result.Failure(Error.Validation("التحليل المحدد ليس مزرعة."));
        }

        var antibiotic = _db.Set<Antibiotic>().FirstOrDefault(a => a.Id.Value == request.AntibioticId);
        if (antibiotic is null)
        {
            return Result.Failure(Error.NotFound("المضاد الحيوي غير موجود."));
        }

        if (_db.Set<CultureAntibioticAttachment>().Any(a =>
                a.TestId.Value == request.TestId
                && a.AntibioticId.Value == request.AntibioticId))
        {
            return Result.Failure(Error.Conflict("المضاد الحيوي مضاف بالفعل لهذه المزرعة."));
        }

        _db.Add(new CultureAntibioticAttachment(
            TestId.Create(request.TestId),
            AntibioticId.Create(request.AntibioticId)));

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}