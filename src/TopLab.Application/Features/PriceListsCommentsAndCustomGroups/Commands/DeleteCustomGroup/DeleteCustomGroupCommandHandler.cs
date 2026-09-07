using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeleteCustomGroup;

public sealed class DeleteCustomGroupCommandHandler : IRequestHandler<DeleteCustomGroupCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeleteCustomGroupCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(DeleteCustomGroupCommand request, CancellationToken cancellationToken)
    {
        var group = _db.Set<CustomGroup>().FirstOrDefault(g => g.Id.Value == request.Id);
        if (group is null)
        {
            return Result.Failure(Error.NotFound("المجموعة غير موجودة."));
        }

        _db.Remove(group);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
