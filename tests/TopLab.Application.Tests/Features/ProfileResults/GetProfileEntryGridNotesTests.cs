using TopLab.Application.Features.ProfileResults.Queries.GetProfileEntryGrid;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Application.Tests.Features.ProfileResults;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ProfileResults;

public class GetProfileEntryGridNotesTests
{
    [Fact]
    public async Task CarriesNotes_WhenPresent()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "P1", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow, notes: "profile note"));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(10));
        ProfileResultsSeed.SeedAnalyteWithRange(db, 1);
        ProfileResultsSeed.AddProfile(db, 10, 1);
        var pt = ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);

        var handler = new GetProfileEntryGridQueryHandler(db);
        var result = await handler.Handle(new GetProfileEntryGridQuery(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("profile note", result.Value!.PatientNotes);
    }

    [Fact]
    public async Task CarriesNull_WhenAbsent()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(10));
        ProfileResultsSeed.SeedAnalyteWithRange(db, 1);
        ProfileResultsSeed.AddProfile(db, 10, 1);
        var pt = ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);

        var handler = new GetProfileEntryGridQueryHandler(db);
        var result = await handler.Handle(new GetProfileEntryGridQuery(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.PatientNotes);
    }
}
