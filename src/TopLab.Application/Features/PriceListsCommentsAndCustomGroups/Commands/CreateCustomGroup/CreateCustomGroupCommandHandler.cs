using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateCustomGroup;

public sealed class CreateCustomGroupCommandHandler : IRequestHandler<CreateCustomGroupCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public CreateCustomGroupCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(CreateCustomGroupCommand request, CancellationToken cancellationToken)
    {
        if (_db.Set<CustomGroup>().Any(g => g.Name == request.Name))
        {
            return Result<int>.Failure(Error.Conflict("المجموعة موجودة بالفعل"));
        }

        CustomGroup group;
        try
        {
            group = CustomGroup.Create(CustomGroupId.Create(0), request.Name);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        _db.Add(group);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(group.Id.Value);
    }
}
