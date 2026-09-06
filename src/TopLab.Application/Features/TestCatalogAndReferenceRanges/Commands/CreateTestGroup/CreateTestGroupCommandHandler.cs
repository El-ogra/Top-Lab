using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTestGroup;

public sealed class CreateTestGroupCommandHandler : IRequestHandler<CreateTestGroupCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public CreateTestGroupCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(CreateTestGroupCommand request, CancellationToken cancellationToken)
    {
        if (_db.Set<TestGroup>().Any(g => g.Name == request.Name))
        {
            return Result<int>.Failure(Error.Conflict("مجموعة التحاليل موجودة بالفعل"));
        }

        var group = TestGroup.Create(TestGroupId.Create(0), request.Name);
        _db.Add(group);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(group.Id.Value);
    }
}