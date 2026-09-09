using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UnmapTestFromAnalyte;

public sealed class UnmapTestFromAnalyteCommandHandler : IRequestHandler<UnmapTestFromAnalyteCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UnmapTestFromAnalyteCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UnmapTestFromAnalyteCommand request, CancellationToken cancellationToken)
    {
        var test = _db.Set<Test>().FirstOrDefault(t => t.Id.Value == request.TestId);
        if (test is null)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        test.ClearAnalyteMapping();

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}