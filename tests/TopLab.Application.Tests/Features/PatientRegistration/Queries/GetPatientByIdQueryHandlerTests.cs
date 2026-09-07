using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientById;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Queries;

public class GetPatientByIdQueryHandlerTests
{
    private static Patient MakePatient(int id, string name = "Ahmed")
    {
        return Patient.Create(PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_ReturnsFullDto()
    {
        var db = new FakeApplicationDbContext();
        var p = MakePatient(1);
        p.SetPhoneNumbers(new[] { new PatientNumberInput("01012345678", 0) });
        p.AddMedicalCondition(MedicalConditionTypeId.Create(1));
        db.Patients.Add(p);
        db.PatientPhoneNumbers.Add(PatientPhoneNumber.Create(PatientPhoneNumberId.Create(1), PatientId.Create(1), "01012345678", 0));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(1), "Diabetes", MedicalConditionCategory.Condition));
        db.PatientMedicalConditions.Add(PatientMedicalCondition.Create(PatientId.Create(1), MedicalConditionTypeId.Create(1)));

        var handler = new GetPatientByIdQueryHandler(db);
        var result = await handler.Handle(new GetPatientByIdQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.PatientId);
        Assert.Equal("Ahmed", result.Value.FullName);
        Assert.Single(result.Value.PhoneNumbers);
        Assert.Single(result.Value.MedicalConditions);
    }

    [Fact]
    public async Task Handle_UnknownId_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetPatientByIdQueryHandler(db);

        var result = await handler.Handle(new GetPatientByIdQuery(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_SoftDeleted_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var p = MakePatient(1);
        p.SoftDelete();
        db.Patients.Add(p);

        var handler = new GetPatientByIdQueryHandler(db);
        var result = await handler.Handle(new GetPatientByIdQuery(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_WithReferralEntity_ResolvesName()
    {
        var db = new FakeApplicationDbContext();
        var p = MakePatient(1);
        p.SetReferralEntity(ExternalEntityId.Create(7));
        db.Patients.Add(p);
        db.ExternalEntities.Add(ExternalEntity.Create(ExternalEntityId.Create(7), EntityType.ReferralOrContract, "Alpha Lab", null, null, null, null, null, null, PriceListId.Create(1), null));

        var handler = new GetPatientByIdQueryHandler(db);
        var result = await handler.Handle(new GetPatientByIdQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value!.ReferralEntityId);
        Assert.Equal("Alpha Lab", result.Value.ReferralEntityName);
    }
}