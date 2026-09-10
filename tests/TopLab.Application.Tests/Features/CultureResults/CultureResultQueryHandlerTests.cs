using TopLab.Application.Features.CultureResults.Queries.GetCultureEntryGrid;
using TopLab.Application.Features.CultureResults.Queries.GetCultureReport;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureResults;

public class CultureResultQueryHandlerTests
{
    [Fact]
    public async Task Grid_StrictlyFiltersChildrenButKeepsSavedFacts()
    {
        var db = Seed(age: 12);
        db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(CultureAntibioticResultId.Create(10), PatientTestId.Create(10), AntibioticId.Create(2), SensitivityCategory.ResistantFor));

        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Collection(result.Value!.Rows,
            row => Assert.Equal(1, row.AntibioticId),
            row => { Assert.Equal(2, row.AntibioticId); Assert.Equal(10, row.CultureAntibioticResultId); });
    }

    [Fact]
    public async Task Grid_ShowsChildrenFlaggedAntibiotic_WhenChildIsUnder12()
    {
        var db = Seed(age: 11);
        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);
        Assert.Equal(new[] { 1, 2 }, result.Value!.Rows.Select(x => x.AntibioticId));
    }

    [Fact]
    public async Task Report_UsesSavedRowsOnlyAndEchoesSetting()
    {
        var db = Seed(25);
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.CultureAntibioticResults.Add(CultureAntibioticResult.Create(CultureAntibioticResultId.Create(10), PatientTestId.Create(10), AntibioticId.Create(1), SensitivityCategory.HighlyFor));
        var result = await new GetCultureReportQueryHandler(db).Handle(new GetCultureReportQuery(10), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Rows);
        Assert.Equal(1, result.Value.Rows[0].AntibioticId);
        Assert.False(result.Value.PrintLabIdInsteadOfPatientId);
    }

    private static FakeApplicationDbContext Seed(int age)
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Patient", Sex.Female, age, AgeUnit.Year, DateTime.UtcNow));
        db.Tests.Add(Test.Create(TestId.Create(1), "Culture", "Culture report", "Culture", "CULT", 1, 100m, ResultKind.Culture, isCultureType: true));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(10), PatientId.Create(1), TestId.Create(1), 100m));
        db.Antibiotics.Add(Antibiotic.Create(AntibioticId.Create(1), "Normal"));
        db.Antibiotics.Add(Antibiotic.Create(AntibioticId.Create(2), "Child", isChildrenFlagged: true));
        db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(1)));
        db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(2)));
        return db;
    }
}
