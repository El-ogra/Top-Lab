using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.MapTestToAnalyte;

public sealed class MapTestToAnalyteCommandHandler : IRequestHandler<MapTestToAnalyteCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public MapTestToAnalyteCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(MapTestToAnalyteCommand request, CancellationToken cancellationToken)
    {
        var test = _db.Set<Test>().FirstOrDefault(t => t.Id.Value == request.TestId);
        if (test is null)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        if (!_db.Set<Analyte>().Any(a => a.Id.Value == request.AnalyteId))
        {
            return Result.Failure(Error.NotFound("المادة التحليلية غير موجودة"));
        }

        try
        {
            test.MapToAnalyte(AnalyteId.Create(request.AnalyteId));
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(Error.Conflict("لا يمكن ربط المادة التحليلية إلا بتحليل بسيط."));
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}