using TopLab.Application.Features.CultureResults.Queries.GetCultureEntryGrid;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureResults;

public class GetCultureEntryGridNotesTests
{
    private static FakeApplicationDbContext Seed(bool withNotes)
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Patient", Sex.Female, 30, AgeUnit.Year, DateTime.UtcNow, notes: withNotes ? "culture note" : null));
        db.Tests.Add(Test.Create(TestId.Create(1), "Culture", "Culture report", "Culture", "CULT", 1, 100m, ResultKind.Culture, isCultureType: true));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(10), PatientId.Create(1), TestId.Create(1), 100m));
        db.Antibiotics.Add(Antibiotic.Create(AntibioticId.Create(1), "Normal"));
        db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(1)));
        return db;
    }

    [Fact]
    public async Task CarriesNotes_WhenPresent()
    {
        var db = Seed(true);
        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("culture note", result.Value!.PatientNotes);
    }

    [Fact]
    public async Task CarriesNull_WhenAbsent()
    {
        var db = Seed(false);
        var result = await new GetCultureEntryGridQueryHandler(db).Handle(new GetCultureEntryGridQuery(10), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.PatientNotes);
    }
}
