using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreatePriceList;

public sealed class CreatePriceListCommandHandler : IRequestHandler<CreatePriceListCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public CreatePriceListCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(CreatePriceListCommand request, CancellationToken cancellationToken)
    {
        if (_db.Set<PriceList>().Any(p => p.Name == request.Name))
        {
            return Result<int>.Failure(Error.Conflict("قائمة أسعار موجودة بالفعل"));
        }

        PriceList list;
        try
        {
            list = PriceList.Create(PriceListId.Create(0), request.Name);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        _db.Add(list);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(list.Id.Value);
    }
}
