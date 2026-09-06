using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Application.Features.ExternalEntities.Common;

namespace TopLab.Application.Features.ExternalEntities.Commands.UpdateExternalEntity;

public sealed class UpdateExternalEntityCommandHandler : IRequestHandler<UpdateExternalEntityCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateExternalEntityCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UpdateExternalEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = _db.Set<ExternalEntity>().FirstOrDefault(e => e.Id.Value == request.Id);
        if (entity is null)
        {
            return Result.Failure(Error.NotFound("الجهة الخارجية غير موجودة."));
        }

        if (request.PriceListId.HasValue
            && !_db.Set<PriceList>().Any(p => p.Id.Value == request.PriceListId.Value))
        {
            return Result.Failure(Error.NotFound("قائمة الأسعار المحددة غير موجودة."));
        }

        try
        {
            entity.Update(
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
            return Result.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
