using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;

namespace TopLab.Application.Features.ExternalEntities.Commands.CreateExternalEntity;

public sealed class CreateExternalEntityCommandHandler : IRequestHandler<CreateExternalEntityCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public CreateExternalEntityCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(CreateExternalEntityCommand request, CancellationToken cancellationToken)
    {
        if (request.PriceListId.HasValue
            && !_db.Set<PriceList>().Any(p => p.Id.Value == request.PriceListId.Value))
        {
            return Result<int>.Failure(Error.NotFound("قائمة الأسعار المحددة غير موجودة."));
        }

        ExternalEntity entity;
        try
        {
            entity = ExternalEntity.Create(
                ExternalEntityId.Create(0),
                request.EntityType,
                request.Name,
                request.City,
                request.Address,
                request.Phone,
                request.Fax,
                request.ResponsiblePersonName,
                request.ResponsiblePersonPhone,
                request.PriceListId.HasValue ? PriceListId.Create(request.PriceListId.Value) : null,
                request.DiscountOrCommissionPercent);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        _db.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id.Value);
    }
}
