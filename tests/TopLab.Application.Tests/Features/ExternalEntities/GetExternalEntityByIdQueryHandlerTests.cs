using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Queries.GetExternalEntityById;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using Xunit;

namespace TopLab.Application.Tests.Features.ExternalEntities;

public class GetExternalEntityByIdQueryHandlerTests
{
    private static FakeApplicationDbContext BuildDb()
    {
        var db = new FakeApplicationDbContext();
        db.Add(PriceList.Create(PriceListId.Create(1), "Standard"));
        db.Add(ExternalEntity.Create(
            ExternalEntityId.Create(5), EntityType.ReferralOrContract, "Delta Contract",
            city: "Tanta", address: "Street 1", phone: "0403334444", fax: "040555",
            responsiblePersonName: "Omar", responsiblePersonPhone: "0111000",
            priceListId: PriceListId.Create(1), discountOrCommissionPercent: 5m));
        return db;
    }

    [Fact]
    public async Task GetById_Found_ReturnsDetail()
    {
        var handler = new GetExternalEntityByIdQueryHandler(BuildDb());

        var result = await handler.Handle(new GetExternalEntityByIdQuery(5), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal(5, dto.Id);
        Assert.Equal(EntityType.ReferralOrContract, dto.EntityType);
        Assert.Equal("Delta Contract", dto.Name);
        Assert.Equal("Tanta", dto.City);
        Assert.Equal("Street 1", dto.Address);
        Assert.Equal("0403334444", dto.Phone);
        Assert.Equal("040555", dto.Fax);
        Assert.Equal("Omar", dto.ResponsiblePersonName);
        Assert.Equal("0111000", dto.ResponsiblePersonPhone);
        Assert.Equal(1, dto.PriceListId);
        Assert.Equal("Standard", dto.PriceListName);
        Assert.Equal(5m, dto.DiscountOrCommissionPercent);
    }

    [Fact]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        var handler = new GetExternalEntityByIdQueryHandler(BuildDb());

        var result = await handler.Handle(new GetExternalEntityByIdQuery(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("الجهة الخارجية غير موجودة.", result.Error.Message);
    }
}
