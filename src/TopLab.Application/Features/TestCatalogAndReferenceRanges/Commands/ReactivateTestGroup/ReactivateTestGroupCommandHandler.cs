using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.ReactivateTestGroup;

public sealed class ReactivateTestGroupCommandHandler : IRequestHandler<ReactivateTestGroupCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public ReactivateTestGroupCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(ReactivateTestGroupCommand request, CancellationToken cancellationToken)
    {
        var group = _db.Set<TestGroup>().FirstOrDefault(g => g.Id.Value == request.Id);
        if (group is null)
        {
            return Result.Failure(Error.NotFound("مجموعة التحاليل غير موجودة"));
        }

        if (group.IsActive)
        {
            return Result.Failure(Error.Conflict("المجموعة نشطة بالفعل"));
        }

        group.Reactivate();
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}