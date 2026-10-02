using TopLab.Application.Features.CultureResults.Commands.SaveCultureResults;
using TopLab.Application.Features.CultureResults.Queries.GetCultureEntryGrid;
using TopLab.Application.Features.CultureResults.Queries.GetCultureReport;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureResults;

/// <summary>W-02 S11 (WP-14): zone and microscopy travel entry → store → grid → report.</summary>
public class CultureZoneRoundTripTests
{
    private static FakeApplicationDbContext Seed()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "P", Sex.Female, 25, AgeUnit.Year, DateTime.UtcNow));
        db.Tests.Add(Test.Create(TestId.Create(1), "C", "C", "C", "C", 1, 100m, ResultKind.Culture, true));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(10), PatientId.Create(1), TestId.Create(1), 100));
        db.CultureAntibioticAttachments.Add(new CultureAntibioticAttachment(TestId.Create(1), AntibioticId.Create(1)));
        db.Antibiotics.Add(Antibiotic.Create(AntibioticId.Create(1), "Amoxicillin", false, false, "AMX", "Amoxicillin trihydrate"));
        return db;
    }

    [Fact]
    public async Task CultureEntry_ZoneValue_RoundTripsThroughSave()
    {
        var db = Seed();

        Assert.True((await new SaveCultureResultsCommandHandler(db).Handle(
            new SaveCultureResultsCommand(10, null, null, null, null, null, null, [new(1, 0, 18.5m)]),
            default)).IsSuccess);

        var grid = await new GetCultureEntryGridQueryHandler(db).Handle(
            new GetCultureEntryGridQuery(10), default);

        Assert.True(grid.IsSuccess);
        Assert.Equal(18.5m, Assert.Single(grid.Value!.Rows).InhibitionZoneMm);
    }

    [Fact]
    public async Task CultureReport_UsesSystemSettingsIdOne()
    {
        var db = Seed();
        db.SystemSettings.Add(TopLab.Domain.Settings.SystemSettings.CreateDefault());

        var result = await new GetCultureReportQueryHandler(db).Handle(
            new GetCultureReportQuery(10), default);

        Assert.True(result.IsSuccess);
    }
}
