using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Commands.UpdateExternalEntity;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using Xunit;

namespace TopLab.Application.Tests.Features.ExternalEntities;

public class UpdateExternalEntityCommandHandlerTests
{
    private static FakeApplicationDbContext BuildDb()
    {
        var db = new FakeApplicationDbContext();
        db.Add(PriceList.Create(PriceListId.Create(1), "Standard"));
        db.Add(ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr. Ahmed", city: "Cairo"));
        db.Add(ExternalEntity.Create(
            ExternalEntityId.Create(2), EntityType.ReferralOrContract, "Delta",
            priceListId: PriceListId.Create(1)));
        return db;
    }

    private static UpdateExternalEntityCommand Update(int id, EntityType type, string name = "Dr. Ahmed",
        int? priceListId = null, decimal? percent = null) =>
        new(id, type, name, "Cairo", null, "0100", null, null, null, priceListId, percent);

    [Fact]
    public async Task Update_DoctorFields_Succeeds()
    {
        var db = BuildDb();
        var handler = new UpdateExternalEntityCommandHandler(db);

        var result = await handler.Handle(
            Update(1, EntityType.TreatingDoctor, "Dr. Mahmoud", percent: 15m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Dr. Mahmoud", db.ExternalEntities[0].Name);
        Assert.Equal(15m, db.ExternalEntities[0].DiscountOrCommissionPercent);
    }

    [Fact]
    public async Task Update_Missing_ReturnsNotFound()
    {
        var handler = new UpdateExternalEntityCommandHandler(BuildDb());

        var result = await handler.Handle(
            Update(999, EntityType.TreatingDoctor), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("الجهة الخارجية غير موجودة.", result.Error.Message);
    }

    [Fact]
    public async Task Update_DoctorToReferralWithoutList_Fails()
    {
        var handler = new UpdateExternalEntityCommandHandler(BuildDb());

        var result = await handler.Handle(
            Update(1, EntityType.ReferralOrContract), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("جهة الإحالة / التعاقد تتطلب قائمة أسعار.", result.Error!.Message);
    }

    [Fact]
    public async Task Update_ReferralToDoctorWithList_Fails()
    {
        var handler = new UpdateExternalEntityCommandHandler(BuildDb());

        var result = await handler.Handle(
            Update(2, EntityType.TreatingDoctor, "Dr", priceListId: 1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("الطبيب المعالج لا يرتبط بقائمة أسعار.", result.Error!.Message);
    }

    [Fact]
    public async Task Update_UnknownList_ReturnsNotFound()
    {
        var handler = new UpdateExternalEntityCommandHandler(BuildDb());

        var result = await handler.Handle(
            Update(2, EntityType.ReferralOrContract, "Delta", priceListId: 999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("قائمة الأسعار المحددة غير موجودة.", result.Error.Message);
    }

    [Fact]
    public async Task Update_PercentOutOfRange_Fails()
    {
        var handler = new UpdateExternalEntityCommandHandler(BuildDb());

        var result = await handler.Handle(
            Update(1, EntityType.TreatingDoctor, percent: 150m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("نسبة الخصم / العمولة يجب أن تكون بين 0 و 100.", result.Error!.Message);
    }
}
