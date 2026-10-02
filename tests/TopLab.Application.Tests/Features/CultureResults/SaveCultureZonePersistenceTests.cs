using TopLab.Application.Features.CultureResults.Commands.SaveCultureResults;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureResults;

/// <summary>W-02 S9 (WP-14): the S2 update-in-place path must preserve the
/// inhibition zone across re-saves (SD-13).</summary>
public class SaveCultureZonePersistenceTests
{
    private static FakeApplicationDbContext Seed()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "P", Sex.Female, 25, AgeUnit.Year, DateTime.UtcNow));
        db.Tests.Add(Test.Create(TestId.Create(1), "C", "C", "C", "C", 1, 100m, ResultKind.Culture, true));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(10), PatientId.Create(1), TestId.Create(1), 100));
        db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(1)));
        return db;
    }

    [Fact]
    public async Task SaveCulture_ZoneValue_SurvivesReSave()
    {
        var db = Seed();
        var save = new SaveCultureResultsCommandHandler(db);

        Assert.True((await save.Handle(
            new SaveCultureResultsCommand(10, null, null, null, null, null, null, [new(1, (int)SensitivityCategory.HighlyFor)]),
            default)).IsSuccess);

        var row = Assert.Single(db.CultureAntibioticResults);
        row.SetInhibitionZone(18.5m);

        Assert.True((await save.Handle(
            new SaveCultureResultsCommand(10, null, null, null, null, null, null, [new(1, (int)SensitivityCategory.HighlyFor, 18.5m)]),
            default)).IsSuccess);

        Assert.Equal(18.5m, Assert.Single(db.CultureAntibioticResults).InhibitionZoneMm);
    }

    [Fact]
    public async Task SaveCulture_ExplicitNullZone_ClearsStoredZone()
    {
        var db = Seed();
        var save = new SaveCultureResultsCommandHandler(db);

        Assert.True((await save.Handle(
            new SaveCultureResultsCommand(10, null, null, null, null, null, null, [new(1, (int)SensitivityCategory.HighlyFor, 18.5m)]),
            default)).IsSuccess);
        Assert.Equal(18.5m, Assert.Single(db.CultureAntibioticResults).InhibitionZoneMm);

        Assert.True((await save.Handle(
            new SaveCultureResultsCommand(10, null, null, null, null, null, null, [new(1, (int)SensitivityCategory.HighlyFor)]),
            default)).IsSuccess);

        Assert.Null(Assert.Single(db.CultureAntibioticResults).InhibitionZoneMm);
    }
}
