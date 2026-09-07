using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.SoftDeletePatient;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class SoftDeletePatientCommandHandlerTests
{
    [Fact]
    public async Task SoftDelete_HappyPath_SetsIsDeleted()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        db.Patients.Add(p);

        var handler = new SoftDeletePatientCommandHandler(db);
        var result = await handler.Handle(new SoftDeletePatientCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(p.IsDeleted);
    }

    [Fact]
    public async Task SoftDelete_DoubleDelete_Idempotent()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        db.Patients.Add(p);

        var handler = new SoftDeletePatientCommandHandler(db);
        await handler.Handle(new SoftDeletePatientCommand(1), CancellationToken.None);
        var second = await handler.Handle(new SoftDeletePatientCommand(1), CancellationToken.None);

        Assert.True(second.IsSuccess);
        Assert.True(p.IsDeleted);
    }

    [Fact]
    public async Task SoftDelete_UnknownPatient_NotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new SoftDeletePatientCommandHandler(db);

        var result = await handler.Handle(new SoftDeletePatientCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}