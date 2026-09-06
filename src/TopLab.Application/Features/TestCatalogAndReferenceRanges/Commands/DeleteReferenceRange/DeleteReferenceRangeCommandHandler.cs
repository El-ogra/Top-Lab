using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeleteReferenceRange;

public sealed class DeleteReferenceRangeCommandHandler : IRequestHandler<DeleteReferenceRangeCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeleteReferenceRangeCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(DeleteReferenceRangeCommand request, CancellationToken cancellationToken)
    {
        var range = _db.Set<ReferenceRange>().FirstOrDefault(r => r.Id.Value == request.Id);
        if (range is null)
        {
            return Result.Failure(Error.NotFound("النطاق المرجعي غير موجود"));
        }

        _db.Remove(range);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}