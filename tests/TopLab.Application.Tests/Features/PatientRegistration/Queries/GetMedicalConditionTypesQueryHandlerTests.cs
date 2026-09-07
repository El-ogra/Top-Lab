using TopLab.Application.Features.PatientRegistration.Queries.GetMedicalConditionTypes;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Queries;

public class GetMedicalConditionTypesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsByCategory()
    {
        var db = new FakeApplicationDbContext();
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(1), "Diabetes", MedicalConditionCategory.Condition));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(2), "Pregnant", MedicalConditionCategory.Medication));

        var handler = new GetMedicalConditionTypesQueryHandler(db);
        var result = await handler.Handle(new GetMedicalConditionTypesQuery(MedicalConditionCategory.Condition), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Diabetes", result.Value![0].Name);
    }

    [Fact]
    public async Task Handle_NoCategory_ReturnsAll()
    {
        var db = new FakeApplicationDbContext();
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(1), "Diabetes", MedicalConditionCategory.Condition));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(2), "Pregnant", MedicalConditionCategory.Medication));

        var handler = new GetMedicalConditionTypesQueryHandler(db);
        var result = await handler.Handle(new GetMedicalConditionTypesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }
}