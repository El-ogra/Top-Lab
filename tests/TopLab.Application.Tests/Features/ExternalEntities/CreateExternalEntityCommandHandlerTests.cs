using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Commands.CreateExternalEntity;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Application.Tests.Features.ExternalEntities;

public class CreateExternalEntityCommandHandlerTests
{
    private static FakeApplicationDbContext BuildDb()
    {
        var db = new FakeApplicationDbContext();
        db.Add(PriceList.Create(PriceListId.Create(1), "Standard"));
        return db;
    }

    private static CreateExternalEntityCommand Doctor(string name = "Dr. Ahmed", decimal? percent = null) =>
        new(EntityType.TreatingDoctor, name, "Cairo", null, "0100", null, null, null, null, percent);

    [Fact]
    public async Task Create_DoctorWithoutList_Succeeds()
    {
        var db = BuildDb();
        var handler = new CreateExternalEntityCommandHandler(db);

        var result = await handler.Handle(Doctor(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, db.ExternalEntities.Count);
        Assert.Equal("Dr. Ahmed", db.ExternalEntities[0].Name);
        Assert.Null(db.ExternalEntities[0].PriceListId);
    }

    [Fact]
    public async Task Create_DoctorWithList_FailsValidation()
    {
        var handler = new CreateExternalEntityCommandHandler(BuildDb());
        var command = Doctor() with { PriceListId = 1 };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("الطبيب المعالج لا يرتبط بقائمة أسعار.", result.Error.Message);
    }

    [Fact]
    public async Task Create_ReferralWithoutList_FailsValidation()
    {
        var handler = new CreateExternalEntityCommandHandler(BuildDb());
        var command = new CreateExternalEntityCommand(
            EntityType.ReferralOrContract, "Delta", null, null, null, null, null, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("جهة الإحالة / التعاقد تتطلب قائمة أسعار.", result.Error.Message);
    }

    [Fact]
    public async Task Create_ReferralWithUnknownList_ReturnsNotFound()
    {
        var handler = new CreateExternalEntityCommandHandler(BuildDb());
        var command = new CreateExternalEntityCommand(
            EntityType.ReferralOrContract, "Delta", null, null, null, null, null, null, 999, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("قائمة الأسعار المحددة غير موجودة.", result.Error.Message);
    }

    [Fact]
    public async Task Create_ReferralWithList_Succeeds()
    {
        var db = BuildDb();
        var handler = new CreateExternalEntityCommandHandler(db);
        var command = new CreateExternalEntityCommand(
            EntityType.ReferralOrContract, "Delta", "Tanta", null, "040", null, "Omar", "0111", 1, 5m);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PriceListId.Create(1), db.ExternalEntities[0].PriceListId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Create_PartnerLab_WithOrWithoutList_Succeeds(bool withList)
    {
        var db = BuildDb();
        var handler = new CreateExternalEntityCommandHandler(db);
        var command = new CreateExternalEntityCommand(
            EntityType.PartnerLab, "Alpha Lab", null, null, null, null, null, null, withList ? 1 : null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task Create_PercentOutOfRange_FailsValidation(decimal percent)
    {
        var handler = new CreateExternalEntityCommandHandler(BuildDb());

        var result = await handler.Handle(Doctor(percent: percent), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("نسبة الخصم / العمولة يجب أن تكون بين 0 و 100.", result.Error!.Message);
    }

    [Fact]
    public async Task Create_EmptyName_FailsValidation()
    {
        var handler = new CreateExternalEntityCommandHandler(BuildDb());

        var result = await handler.Handle(Doctor(name: "  "), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("اسم الجهة الخارجية مطلوب.", result.Error!.Message);
    }
}
