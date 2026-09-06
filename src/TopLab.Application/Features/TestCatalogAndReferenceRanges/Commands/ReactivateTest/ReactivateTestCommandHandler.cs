using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.ReactivateTest;

public sealed class ReactivateTestCommandHandler : IRequestHandler<ReactivateTestCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public ReactivateTestCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(ReactivateTestCommand request, CancellationToken cancellationToken)
    {
        var test = _db.Set<Test>().FirstOrDefault(t => t.Id.Value == request.Id);
        if (test is null)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        if (test.IsActive)
        {
            return Result.Failure(Error.Conflict("التحليل نشط بالفعل"));
        }

        test.Reactivate();
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}