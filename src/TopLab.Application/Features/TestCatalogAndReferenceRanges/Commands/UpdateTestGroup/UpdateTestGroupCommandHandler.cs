using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateTestGroup;

public sealed class UpdateTestGroupCommandHandler : IRequestHandler<UpdateTestGroupCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateTestGroupCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UpdateTestGroupCommand request, CancellationToken cancellationToken)
    {
        var group = _db.Set<TestGroup>().FirstOrDefault(g => g.Id.Value == request.Id);
        if (group is null)
        {
            return Result.Failure(Error.NotFound("مجموعة التحاليل غير موجودة"));
        }

        group.Rename(request.Name);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}