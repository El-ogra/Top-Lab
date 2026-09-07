using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.AddMedicalCondition;
using TopLab.Application.Features.PatientRegistration.Commands.RemoveMedicalCondition;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class MedicalConditionCommandHandlerTests
{
    [Fact]
    public async Task Add_HappyPath()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        db.Patients.Add(p);
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(7), "Diabetes", MedicalConditionCategory.Condition));

        var handler = new AddMedicalConditionCommandHandler(db);
        var result = await handler.Handle(new AddMedicalConditionCommand(1, 7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(p.MedicalConditions);
    }

    [Fact]
    public async Task Add_UnknownCondition_NotFound()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        db.Patients.Add(p);

        var handler = new AddMedicalConditionCommandHandler(db);
        var result = await handler.Handle(new AddMedicalConditionCommand(1, 99), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Add_SoftDeleted_Conflict()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.SoftDelete();
        db.Patients.Add(p);
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(7), "Diabetes", MedicalConditionCategory.Condition));

        var handler = new AddMedicalConditionCommandHandler(db);
        var result = await handler.Handle(new AddMedicalConditionCommand(1, 7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public async Task Remove_HappyPath()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.AddMedicalCondition(MedicalConditionTypeId.Create(7));
        db.Patients.Add(p);

        var handler = new RemoveMedicalConditionCommandHandler(db);
        var result = await handler.Handle(new RemoveMedicalConditionCommand(1, 7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(p.MedicalConditions);
    }
}