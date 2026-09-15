using TopLab.Application.Features.ReportProduction.Queries.GetMultiPatientHistory;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

public class GetMultiPatientHistoryQueryHandlerTests
{
    [Fact]
    public async Task ByLabCode_UnionsVisitPatientsAndOrdersByLabId()
    {
        var db = Seed();
        // selection of two visits: patient 1 (L-1) and patient 4 (L-2 both via patient5/L-2 relation)
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali A", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-1")));
        db.Patients.Add(Patient.Create(PatientId.Create(2), "Ali B", Sex.Male, 31, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-1")));
        db.Patients.Add(Patient.Create(PatientId.Create(3), "Zed", Sex.Male, 40, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-2")));
        db.Patients.Add(Patient.Create(PatientId.Create(4), "Qys", Sex.Male, 41, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-2")));
        db.Tests.Add(Test.Create(TestId.Create(10), "Glucose", "Glucose report", "Glucose", "GLU", 1, 100m, ResultKind.Simple));
        db.PatientTests.Add(GetPatientTestHistoryQueryHandlerTests.Reviewed(101, patientId: 1, testId: 10, value: "90"));
        db.PatientTests.Add(GetPatientTestHistoryQueryHandlerTests.Reviewed(102, patientId: 4, testId: 10, value: "85", reviewedAt: DateTime.UtcNow.AddMinutes(4)));
        db.PatientTests.Add(GetPatientTestHistoryQueryHandlerTests.Reviewed(103, patientId: 2, testId: 10, value: "80", reviewedAt: DateTime.UtcNow.AddMinutes(2)));
        db.PatientTests.Add(GetPatientTestHistoryQueryHandlerTests.Reviewed(104, patientId: 3, testId: 10, value: "95", reviewedAt: DateTime.UtcNow.AddMinutes(3)));

        var result = await new GetMultiPatientHistoryQueryHandler(db)
            .Handle(new GetMultiPatientHistoryQuery(new[] { 1, 4 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        // union = {1,2,3,4}, ordered by LabId (L-1 pairs then L-2 pairs)
        Assert.Equal(new[] { 1, 2, 3, 4 }, result.Value!.Entries.Select(e => e.PatientId));
        Assert.Equal(new[] { 101, 103, 104, 102 }, result.Value.Entries.Select(e => e.PatientTestId));
        Assert.Equal("ByLabCode", result.Value.HistorySortMode);
    }

    [Fact]
    public async Task ByPatientName_UnionsByName()
    {
        var db = Seed();
        db.ReportSettings.Single().SetHistoryOptions(HistorySortMode.ByPatientName, true);
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ahmed Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        db.Patients.Add(Patient.Create(PatientId.Create(2), "AHMED  ALI", Sex.Male, 31, AgeUnit.Year, DateTime.UtcNow));
        db.Patients.Add(Patient.Create(PatientId.Create(3), "Sara", Sex.Female, 28, AgeUnit.Year, DateTime.UtcNow));
        db.Tests.Add(Test.Create(TestId.Create(10), "Glucose", "Glucose report", "Glucose", "GLU", 1, 100m, ResultKind.Simple));
        db.PatientTests.Add(GetPatientTestHistoryQueryHandlerTests.Reviewed(101, patientId: 1, testId: 10, value: "90"));
        db.PatientTests.Add(GetPatientTestHistoryQueryHandlerTests.Reviewed(102, patientId: 3, testId: 10, value: "87", reviewedAt: DateTime.UtcNow.AddMinutes(1)));
        db.PatientTests.Add(GetPatientTestHistoryQueryHandlerTests.Reviewed(103, patientId: 2, testId: 10, value: "88", reviewedAt: DateTime.UtcNow.AddMinutes(2)));

        var result = await new GetMultiPatientHistoryQueryHandler(db)
            .Handle(new GetMultiPatientHistoryQuery(new[] { 1, 3 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        // union = {1,2,3}; "AHMED  ALI" sorts before "Ahmed Ali" (ordinal, ignore case), then Sara
        Assert.Equal(new[] { 2, 1, 3 }, result.Value!.Entries.Select(e => e.PatientId));
        Assert.Equal(new[] { 103, 101, 102 }, result.Value.Entries.Select(e => e.PatientTestId));
    }

    [Fact]
    public async Task UnknownSelectedId_ReturnsNotFound()
    {
        var db = Seed();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-1")));

        var result = await new GetMultiPatientHistoryQueryHandler(db)
            .Handle(new GetMultiPatientHistoryQuery(new[] { 1, 99 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task MissingSettings_ReturnsUnexpected()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));

        var result = await new GetMultiPatientHistoryQueryHandler(db)
            .Handle(new GetMultiPatientHistoryQuery(new[] { 1 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("سجل إعدادات التقرير مفقود.", result.Error!.Message);
    }

    [Fact]
    public async Task UnresolvableSelectedIdentity_ReturnsTranslatedConflict()
    {
        var db = Seed();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));

        var result = await new GetMultiPatientHistoryQueryHandler(db)
            .Handle(new GetMultiPatientHistoryQuery(new[] { 1 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("تعذر تحديد هوية المريض للتاريخ المرضي.", result.Error!.Message);
    }

    private static FakeApplicationDbContext Seed()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        return db;
    }
}