using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.AddMedicalConditionType;
using TopLab.Application.Features.PatientRegistration.Commands.RenameMedicalConditionType;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

/// <summary>
/// STAGE-01 (baseline 1.2.2.3): the catalogue's maintenance commands add and
/// rename rows; the seeded rows are protected and nothing is deleted.
/// </summary>
public class MedicalConditionTypeMaintenanceTests
{
    [Fact]
    public async Task Add_CreatesTheRow()
    {
        var db = new FakeApplicationDbContext();
        var handler = new AddMedicalConditionTypeCommandHandler(db);

        var result = await handler.Handle(
            new AddMedicalConditionTypeCommand(" علاج مخصص ", MedicalConditionCategory.Medication),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(db.MedicalConditionTypes);
        Assert.Equal("علاج مخصص", row.Name);
        Assert.Equal(MedicalConditionCategory.Medication, row.Category);
    }

    [Fact]
    public async Task Add_DuplicateName_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(
            MedicalConditionTypeId.Create(30), "علاج مخصص", MedicalConditionCategory.Medication));
        var handler = new AddMedicalConditionTypeCommandHandler(db);

        var result = await handler.Handle(
            new AddMedicalConditionTypeCommand("علاج مخصص", MedicalConditionCategory.Medication),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Single(db.MedicalConditionTypes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(12)]
    public async Task Rename_SeededRow_IsRefused(int seededId)
    {
        var db = new FakeApplicationDbContext();
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(
            MedicalConditionTypeId.Create(seededId), "مدمج", MedicalConditionCategory.Medication));
        var handler = new RenameMedicalConditionTypeCommandHandler(db);

        var result = await handler.Handle(
            new RenameMedicalConditionTypeCommand(seededId, "اسم جديد"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("مدمج", db.MedicalConditionTypes[0].Name);
    }

    [Fact]
    public async Task Rename_OperatorRow_RenamesIt()
    {
        var db = new FakeApplicationDbContext();
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(
            MedicalConditionTypeId.Create(30), "قديم", MedicalConditionCategory.Medication));
        var handler = new RenameMedicalConditionTypeCommandHandler(db);

        var result = await handler.Handle(
            new RenameMedicalConditionTypeCommand(30, " جديد "),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("جديد", db.MedicalConditionTypes[0].Name);
    }

    [Fact]
    public async Task Rename_UnknownRow_NotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new RenameMedicalConditionTypeCommandHandler(db);

        var result = await handler.Handle(
            new RenameMedicalConditionTypeCommand(99, "جديد"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Rename_DuplicateName_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(
            MedicalConditionTypeId.Create(30), "قديم", MedicalConditionCategory.Medication));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(
            MedicalConditionTypeId.Create(31), "مستخدم", MedicalConditionCategory.Medication));
        var handler = new RenameMedicalConditionTypeCommandHandler(db);

        var result = await handler.Handle(
            new RenameMedicalConditionTypeCommand(30, "مستخدم"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }
}
