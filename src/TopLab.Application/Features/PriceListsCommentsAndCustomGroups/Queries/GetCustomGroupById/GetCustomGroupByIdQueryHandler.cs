using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupById;

public sealed class GetCustomGroupByIdQueryHandler : IRequestHandler<GetCustomGroupByIdQuery, Result<CustomGroupDetailDto>>
{
    private readonly IApplicationDbContext _db;

    public GetCustomGroupByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<CustomGroupDetailDto>> Handle(GetCustomGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var group = _db.Set<CustomGroup>().FirstOrDefault(g => g.Id.Value == request.Id);
        if (group is null)
        {
            return Task.FromResult(Result<CustomGroupDetailDto>.Failure(Error.NotFound("المجموعة غير موجودة.")));
        }

        var testNamesById = _db.Set<Test>().ToDictionary(t => t.Id, t => (t.Name, t.TestCode));

        var items = _db.Set<CustomGroupItem>()
            .Where(i => i.CustomGroupId.Value == request.Id)
            .ToList()
            .Select(i =>
            {
                testNamesById.TryGetValue(i.TestId, out var info);
                return new CustomGroupItemDto(
                    i.TestId.Value,
                    info.Name,
                    info.TestCode,
                    i.Price);
            })
            .ToList();

        var dto = new CustomGroupDetailDto(group.Id.Value, group.Name, items);

        return Task.FromResult(Result<CustomGroupDetailDto>.Success(dto));
    }
}
