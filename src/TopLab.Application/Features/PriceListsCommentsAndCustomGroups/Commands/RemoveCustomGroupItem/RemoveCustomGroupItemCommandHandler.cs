using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemoveCustomGroupItem;

public sealed class RemoveCustomGroupItemCommandHandler : IRequestHandler<RemoveCustomGroupItemCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public RemoveCustomGroupItemCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(RemoveCustomGroupItemCommand request, CancellationToken cancellationToken)
    {
        var group = _db.Set<CustomGroup>().FirstOrDefault(g => g.Id.Value == request.CustomGroupId);
        if (group is null)
        {
            return Result.Failure(Error.NotFound("المجموعة غير موجودة."));
        }

        var existingRow = _db.Set<CustomGroupItem>()
            .FirstOrDefault(i => i.CustomGroupId.Value == request.CustomGroupId && i.TestId.Value == request.TestId);

        if (existingRow is null)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود في المجموعة."));
        }

        _db.Remove(existingRow);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
