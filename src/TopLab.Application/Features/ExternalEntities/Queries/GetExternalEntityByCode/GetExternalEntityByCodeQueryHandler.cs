using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.ExternalEntities;

namespace TopLab.Application.Features.ExternalEntities.Queries.GetExternalEntityByCode;

public sealed class GetExternalEntityByCodeQueryHandler : IRequestHandler<GetExternalEntityByCodeQuery, Result<ExternalEntityDetailDto>>
{
    private readonly IApplicationDbContext _db;

    public GetExternalEntityByCodeQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<ExternalEntityDetailDto>> Handle(GetExternalEntityByCodeQuery request, CancellationToken cancellationToken)
    {
        var entity = _db.Set<ExternalEntity>().FirstOrDefault(e => e.GeneratedIdCode == request.GeneratedIdCode);
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
