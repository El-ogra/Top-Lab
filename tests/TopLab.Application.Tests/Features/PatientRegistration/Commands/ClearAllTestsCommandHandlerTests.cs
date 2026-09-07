using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.ClearAllTests;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class ClearAllTestsCommandHandlerTests
{
    [Fact]
    public async Task Clear_HappyPath_RemovesAll()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.CreatedAtUtc = DateTime.UtcNow;
        db.Patients.Add(p);
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(10), 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(2), PatientId.Create(1), TestId.Create(11), 200m));

        var handler = new ClearAllTestsCommandHandler(db, new FakeDateTimeProvider { UtcNow = DateTime.UtcNow });
        var result = await handler.Handle(new ClearAllTestsCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.Empty(db.PatientTests);
    }

    [Fact]
    public async Task Clear_TestWithResult_Conflict()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        db.Patients.Add(p);
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(10), 100m);
        pt.EnterResult("5.0", ResultFlag.Normal, 1, DateTime.UtcNow);
        db.PatientTests.Add(pt);

        var handler = new ClearAllTestsCommandHandler(db, new FakeDateTimeProvider());
        var result = await handler.Handle(new ClearAllTestsCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }
}