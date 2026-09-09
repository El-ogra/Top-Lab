using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.CreateAnalyte;

public sealed class CreateAnalyteCommandHandler : IRequestHandler<CreateAnalyteCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public CreateAnalyteCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(CreateAnalyteCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();
        if (_db.Set<Analyte>().Any(a => a.Name == name))
        {
            return Result<int>.Failure(Error.Conflict("اسم المادة التحليلية مستخدم بالفعل"));
        }

        var analyte = Analyte.Create(AnalyteId.Create(0), name ?? string.Empty, request.ReportName);
        var range = AnalyteReferenceRange.Create(AnalyteReferenceRangeId.Create(0), analyte.Id);
        analyte.AttachCurrentRange(range);

        _db.Add(analyte);
        _db.Add(range);

        await _db.SaveChangesAsync(cancellationToken);

        var created = _db.Set<Analyte>().FirstOrDefault(a => a.Id.Value == analyte.Id.Value);
        return created is null
            ? Result<int>.Failure(Error.Unexpected("تعذّر إنشاء المادة التحليلية."))
            : Result<int>.Success(created.Id.Value);
    }
}