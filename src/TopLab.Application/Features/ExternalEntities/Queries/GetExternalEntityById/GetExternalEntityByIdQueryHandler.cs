using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.ExternalEntities;

namespace TopLab.Application.Features.ExternalEntities.Queries.GetExternalEntityById;

public sealed class GetExternalEntityByIdQueryHandler : IRequestHandler<GetExternalEntityByIdQuery, Result<ExternalEntityDetailDto>>
{
    private readonly IApplicationDbContext _db;

    public GetExternalEntityByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<ExternalEntityDetailDto>> Handle(GetExternalEntityByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = _db.Set<ExternalEntity>().FirstOrDefault(e => e.Id.Value == request.Id);
        if (entity is null)
        {
            return Task.FromResult(Result<ExternalEntityDetailDto>.Failure(Error.NotFound("الجهة الخارجية غير موجودة.")));
        }

        var priceListName = entity.PriceListId is not null
            ? _db.Set<PriceList>().FirstOrDefault(p => p.Id.Equals(entity.PriceListId))?.Name
            : null;

        var dto = new ExternalEntityDetailDto(
            entity.Id.Value,
            entity.EntityType,
            entity.Name,
            entity.City,
            entity.Address,
            entity.Phone,
            entity.Fax,
            entity.ResponsiblePersonName,
            entity.ResponsiblePersonPhone,
            entity.PriceListId == null ? null : entity.PriceListId.Value,
            priceListName,
            entity.DiscountOrCommissionPercent,
            entity.GeneratedIdCode);

        return Task.FromResult(Result<ExternalEntityDetailDto>.Success(dto));
    }
}
