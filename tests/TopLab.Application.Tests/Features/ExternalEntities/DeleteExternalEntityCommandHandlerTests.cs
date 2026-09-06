using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Commands.DeleteExternalEntity;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Accounting;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.SentOutSamples;
using Xunit;

namespace TopLab.Application.Tests.Features.ExternalEntities;

public class DeleteExternalEntityCommandHandlerTests
{
    private static FakeApplicationDbContext BuildDb()
    {
        var db = new FakeApplicationDbContext();
        db.Add(PriceList.Create(PriceListId.Create(1), "Standard"));
        db.Add(ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr. Ahmed"));
        db.Add(ExternalEntity.Create(
            ExternalEntityId.Create(2), EntityType.ReferralOrContract, "Delta",
            priceListId: PriceListId.Create(1)));
        db.Add(ExternalEntity.Create(
            ExternalEntityId.Create(3), EntityType.PartnerLab, "Alpha Lab"));
        return db;
    }

    private static Patient BuildPatient(int id, ExternalEntityId? doctorId, ExternalEntityId? referralId) =>
        Patient.Create(PatientId.Create(id), "Patient", Sex.Male, 30, AgeUnit.Year,
            DateTime.UtcNow, treatingDoctorId: doctorId, referralEntityId: referralId);

    [Fact]
    public async Task Delete_Unreferenced_Succeeds()
    {
        var db = BuildDb();
        var handler = new DeleteExternalEntityCommandHandler(db);

        var result = await handler.Handle(new DeleteExternalEntityCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(db.ExternalEntities, e => e.Id.Value == 1);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Delete_Missing_ReturnsNotFound()
    {
        var handler = new DeleteExternalEntityCommandHandler(BuildDb());

        var result = await handler.Handle(new DeleteExternalEntityCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("الجهة الخارجية غير موجودة.", result.Error.Message);
    }

    [Fact]
    public async Task Delete_ReferencedByTreatingDoctor_Blocked()
    {
        var db = BuildDb();
        db.Add(BuildPatient(1, ExternalEntityId.Create(1), null));
        var handler = new DeleteExternalEntityCommandHandler(db);

        var result = await handler.Handle(new DeleteExternalEntityCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("تعذر حذف الجهة لوجود مرضى مرتبطين بها.", result.Error.Message);
        Assert.Contains(db.ExternalEntities, e => e.Id.Value == 1);
    }

    [Fact]
    public async Task Delete_ReferencedByReferralEntity_Blocked()
    {
        var db = BuildDb();
        db.Add(BuildPatient(1, null, ExternalEntityId.Create(2)));
        var handler = new DeleteExternalEntityCommandHandler(db);

        var result = await handler.Handle(new DeleteExternalEntityCommand(2), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("تعذر حذف الجهة لوجود مرضى مرتبطين بها.", result.Error.Message);
    }

    [Fact]
    public async Task Delete_ReferencedBySentOutSample_Blocked()
    {
        var db = BuildDb();
        db.Add(SentOutSample.Create(SentOutSampleId.Create(1), PatientTestId.Create(1),
            ExternalEntityId.Create(3), 50m, 80m, DateTime.UtcNow));
        var handler = new DeleteExternalEntityCommandHandler(db);

        var result = await handler.Handle(new DeleteExternalEntityCommand(3), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("تعذر حذف الجهة لوجود عينات مرسلة مرتبطة بها.", result.Error.Message);
    }

    [Fact]
    public async Task Delete_OnlyCashMovementReference_Succeeds()
    {
        var db = BuildDb();
        db.Add(CashMovement.Create(CashMovementId.Create(1), MovementType.Deposit, 100m, 1,
            DateTime.UtcNow, relatedExternalEntityId: ExternalEntityId.Create(3)));
        var handler = new DeleteExternalEntityCommandHandler(db);

        var result = await handler.Handle(new DeleteExternalEntityCommand(3), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(db.ExternalEntities, e => e.Id.Value == 3);
    }
}
