using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.SetVisitFollowUpFlag;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands.SetVisitFollowUpFlag;

public class SetVisitFollowUpFlagCommandHandlerTests
{
    [Fact]
    public async Task Handle_Success_SetsFlag()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));

        var handler = new SetVisitFollowUpFlagCommandHandler(db);
        var result = await handler.Handle(new SetVisitFollowUpFlagCommand(1, true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(db.Patients.Single().IsFollowUpRequired);
    }

    [Fact]
    public async Task Handle_UnknownPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new SetVisitFollowUpFlagCommandHandler(db);

        var result = await handler.Handle(new SetVisitFollowUpFlagCommand(99, true), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_SoftDeletedPatient_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.SoftDelete();
        db.Patients.Add(p);

        var handler = new SetVisitFollowUpFlagCommandHandler(db);
        var result = await handler.Handle(new SetVisitFollowUpFlagCommand(1, true), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public void Command_RequiresAddEditPatientPermission()
    {
        var cmd = new SetVisitFollowUpFlagCommand(1, true);
        Assert.Equal(PatientRegistrationAccessPolicy.AddEditPatient, cmd.RequiredPermissionCode);
    }
}
