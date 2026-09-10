using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientSearch.Queries.GetPatientByLabId;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientSearch;

public class GetPatientByLabIdQueryHandlerTests
{
    private static Patient MakePatient(int id, string name, string? labId, DateTime when)
    {
        var p = Patient.Create(PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, when);
        if (labId != null)
        {
            p.AssignLabId(LabId.Create(labId));
        }
        return p;
    }

    [Fact]
    public async Task Returns_AllVisitsSharingLabId_LatestFirst()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());

        var older = MakePatient(1, "First", "LAB-1", DateTime.UtcNow.AddDays(-2));
        var newer = MakePatient(2, "Second", "LAB-1", DateTime.UtcNow.AddDays(-1));
        db.Patients.Add(older);
        db.Patients.Add(newer);
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(2), TestId.Create(11), 200m));

        var handler = new GetPatientByLabIdQueryHandler(db);
        var result = await handler.Handle(new GetPatientByLabIdQuery("LAB-1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var history = result.Value!;
        Assert.Equal("LAB-1", history.LabId);
        Assert.Equal("Second", history.PatientFullName);
        Assert.Equal(2, history.Visits.Count);
        Assert.Equal(2, history.Visits[0].PatientId);
        Assert.Equal(1, history.Visits[1].PatientId);
        Assert.Equal(1, history.Visits[0].TestCount);
        Assert.Equal(1, history.Visits[1].TestCount);
    }

    [Fact]
    public async Task Echoes_SettingsValues()
    {
        var db = new FakeApplicationDbContext();
        var settings = ReportSettings.CreateDefault();
        settings.SetHistoryOptions(HistorySortMode.ByPatientName, false);
        db.ReportSettings.Add(settings);
        db.Patients.Add(MakePatient(1, "A", "LAB-5", DateTime.UtcNow));

        var handler = new GetPatientByLabIdQueryHandler(db);
        var result = await handler.Handle(new GetPatientByLabIdQuery("LAB-5"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ByPatientName", result.Value!.HistorySortMode);
        Assert.False(result.Value!.HistoryAutoDisplayEnabled);
    }

    [Fact]
    public async Task UnknownLabId_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        db.Patients.Add(MakePatient(1, "A", "LAB-1", DateTime.UtcNow));

        var handler = new GetPatientByLabIdQueryHandler(db);
        var result = await handler.Handle(new GetPatientByLabIdQuery("LAB-99"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("لا يوجد مريض بهذا الكود.", result.Error!.Message);
    }

    [Fact]
    public async Task SoftDeletedLatestVisit_SkippedToNextNonDeleted()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        var older = MakePatient(1, "Older", "LAB-1", DateTime.UtcNow.AddDays(-2));
        var newerDeleted = MakePatient(2, "NewerDeleted", "LAB-1", DateTime.UtcNow.AddDays(-1));
        newerDeleted.SoftDelete();
        db.Patients.Add(older);
        db.Patients.Add(newerDeleted);

        var handler = new GetPatientByLabIdQueryHandler(db);
        var result = await handler.Handle(new GetPatientByLabIdQuery("LAB-1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var history = result.Value!;
        Assert.Equal("Older", history.PatientFullName);
        var visit = Assert.Single(history.Visits);
        Assert.Equal(1, visit.PatientId);
    }

    [Fact]
    public async Task AllVisitsDeleted_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        var p = MakePatient(1, "Gone", "LAB-1", DateTime.UtcNow);
        p.SoftDelete();
        db.Patients.Add(p);

        var handler = new GetPatientByLabIdQueryHandler(db);
        var result = await handler.Handle(new GetPatientByLabIdQuery("LAB-1"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task MissingReportSettings_ReturnsUnexpected()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "A", "LAB-1", DateTime.UtcNow));

        var handler = new GetPatientByLabIdQueryHandler(db);
        var result = await handler.Handle(new GetPatientByLabIdQuery("LAB-1"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
    }
}