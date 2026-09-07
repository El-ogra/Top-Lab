using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientVisitHistory;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Queries;

public class GetPatientVisitHistoryQueryHandlerTests
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
    public async Task Handle_GroupsByVisit_OrderedByDate()
    {
        var db = new FakeApplicationDbContext();

        var visit1Date = DateTime.UtcNow.AddDays(-2);
        var visit2Date = DateTime.UtcNow.AddDays(-1);

        var p1 = MakePatient(1, "Visit1", "LAB-1", visit1Date);
        var p2 = MakePatient(2, "Visit2", "LAB-1", visit2Date);
        db.Patients.Add(p1);
        db.Patients.Add(p2);

        var test1 = Test.Create(TestId.Create(10), "CBC", "CBC", "CBC", "T-CBC", 60, 100m);
        var test2 = Test.Create(TestId.Create(11), "BMP", "BMP", "BMP", "T-BMP", 60, 200m);
        db.Tests.Add(test1);
        db.Tests.Add(test2);

        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(2), TestId.Create(11), 200m));

        var handler = new GetPatientVisitHistoryQueryHandler(db);
        var result = await handler.Handle(new GetPatientVisitHistoryQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal(1, result.Value![0].PatientId);
        Assert.Equal(2, result.Value![1].PatientId);
        Assert.Single(result.Value![0].Tests);
        Assert.Single(result.Value![1].Tests);
    }

    [Fact]
    public async Task Handle_UnknownPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetPatientVisitHistoryQueryHandler(db);

        var result = await handler.Handle(new GetPatientVisitHistoryQuery(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}