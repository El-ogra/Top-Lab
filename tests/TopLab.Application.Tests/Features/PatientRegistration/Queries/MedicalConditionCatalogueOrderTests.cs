using TopLab.Application.Features.PatientRegistration.Queries.GetMedicalConditionTypes;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Queries;

/// <summary>
/// STAGE-01 (baseline 1.2.2.3): the catalogue is returned in the baseline's
/// question order — the six medication toggles, the pregnancy toggle and then
/// the five condition toggles.
/// </summary>
public class MedicalConditionCatalogueOrderTests
{
    private static FakeApplicationDbContext WithCatalogue()
    {
        var db = new FakeApplicationDbContext();

        // Added in a deliberately wrong order.
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(12), "مريض بالذئبة الحمراء", MedicalConditionCategory.Condition));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(2), "هل يؤخذ علاج للسكر", MedicalConditionCategory.Medication));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(8), "مريض بفقر الدم", MedicalConditionCategory.Condition));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(1), "حمل", MedicalConditionCategory.Pregnancy));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(5), "هل يؤخذ علاج للفيروسات", MedicalConditionCategory.Medication));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(3), "هل يؤخذ علاج لسيولة الدم", MedicalConditionCategory.Medication));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(10), "مريض بالضغط الدم", MedicalConditionCategory.Condition));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(4), "هل يؤخذ علاج للغدد", MedicalConditionCategory.Medication));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(9), "مريض بالتهاب المفاصل", MedicalConditionCategory.Condition));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(6), "هل يأخذ مضاد حيوي", MedicalConditionCategory.Medication));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(11), "مريض بالفشل كلوي", MedicalConditionCategory.Condition));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(7), "هل يؤخذ علاج للكبد", MedicalConditionCategory.Medication));

        return db;
    }

    [Fact]
    public async Task Catalogue_IsReturnedInTheBaselineQuestionOrder()
    {
        var db = WithCatalogue();
        var handler = new GetMedicalConditionTypesQueryHandler(db);

        var result = await handler.Handle(new GetMedicalConditionTypesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new[] { 2, 3, 4, 5, 6, 7, 1, 8, 9, 10, 11, 12 },
            result.Value!.Select(r => r.MedicalConditionTypeId).ToArray());
    }

    [Fact]
    public async Task Catalogue_CarriesTheCategoryOfEveryRow()
    {
        var db = WithCatalogue();
        var handler = new GetMedicalConditionTypesQueryHandler(db);

        var result = await handler.Handle(new GetMedicalConditionTypesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var rows = result.Value!.ToDictionary(r => r.MedicalConditionTypeId, r => r.Category);
        Assert.Equal(MedicalConditionCategory.Pregnancy, rows[1]);
        Assert.Equal(MedicalConditionCategory.Medication, rows[2]);
        Assert.Equal(MedicalConditionCategory.Medication, rows[7]);
        Assert.Equal(MedicalConditionCategory.Condition, rows[8]);
        Assert.Equal(MedicalConditionCategory.Condition, rows[12]);
    }

    [Fact]
    public async Task Catalogue_OperatorAddedRowComesAfterTheSeededRows()
    {
        var db = WithCatalogue();
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(30), "علاج مخصص", MedicalConditionCategory.Medication));
        var handler = new GetMedicalConditionTypesQueryHandler(db);

        var result = await handler.Handle(new GetMedicalConditionTypesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var ids = result.Value!.Select(r => r.MedicalConditionTypeId).ToArray();
        Assert.Equal(30, ids[^1]);
    }

    [Fact]
    public async Task Catalogue_CategoryFilterStillWorks()
    {
        var db = WithCatalogue();
        var handler = new GetMedicalConditionTypesQueryHandler(db);

        var result = await handler.Handle(
            new GetMedicalConditionTypesQuery(MedicalConditionCategory.Condition),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 8, 9, 10, 11, 12 }, result.Value!.Select(r => r.MedicalConditionTypeId).ToArray());
    }
}
