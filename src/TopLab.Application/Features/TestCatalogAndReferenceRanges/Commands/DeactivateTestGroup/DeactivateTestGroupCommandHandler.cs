using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeactivateTestGroup;

public sealed class DeactivateTestGroupCommandHandler : IRequestHandler<DeactivateTestGroupCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeactivateTestGroupCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(DeactivateTestGroupCommand request, CancellationToken cancellationToken)
    {
        var group = _db.Set<TestGroup>().FirstOrDefault(g => g.Id.Value == request.Id);
        if (group is null)
        {
            return Result.Failure(Error.NotFound("مجموعة التحاليل غير موجودة"));
        }

        if (!group.IsActive)
        {
            return Result.Failure(Error.Conflict("المجموعة غير نشطة بالفعل"));
        }

        group.Deactivate();

        var groupId = TestGroupId.Create(request.Id);
        var activeMembers = _db.Set<Test>()
            .Where(t => t.TestGroupId != null && t.TestGroupId.Equals(groupId) && t.IsActive)
            .ToList();

        foreach (var member in activeMembers)
        {
            member.Deactivate();
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}