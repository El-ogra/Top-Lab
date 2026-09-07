using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenameCustomGroup;

public sealed class RenameCustomGroupCommandHandler : IRequestHandler<RenameCustomGroupCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public RenameCustomGroupCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(RenameCustomGroupCommand request, CancellationToken cancellationToken)
    {
        var group = _db.Set<CustomGroup>().FirstOrDefault(g => g.Id.Value == request.Id);
        if (group is null)
        {
            return Result.Failure(Error.NotFound("المجموعة غير موجودة."));
        }

        if (_db.Set<CustomGroup>().Any(g => g.Id.Value != request.Id && g.Name == request.Name))
        {
            return Result.Failure(Error.Conflict("المجموعة موجودة بالفعل"));
        }

        try
        {
            group.Rename(request.Name);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
