using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SampleCollection.Commands.SavePatientSamples;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.SampleCollection;

public class SavePatientSamplesCommandHandlerTests
{
    private static Patient ActivePatient(int id = 1) =>
        Patient.Create(PatientId.Create(id), "أحمد محمد", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);

    private static PatientTest DrawnTest(int id, int patientId, bool drawn = true, bool outsideLab = false)
    {
        var pt = PatientTest.Create(PatientTestId.Create(id), PatientId.Create(patientId), TestId.Create(1), 100m);
        pt.UpdateSampleFlags(isUrine: false, isStool: false, isBlood: false, isSemen: false, isCsf: false, isTakenOutsideLab: outsideLab);
        if (drawn) pt.MarkSampleDrawn(DateTime.UtcNow);
        return pt;
    }

    [Fact]
    public async Task MarksNewRows_Drawn()
    {
        var db = new FakeApplicationDbContext();
        var patient = ActivePatient();
        db.Patients.Add(patient);
        db.PatientTests.Add(DrawnTest(1, patient.Id.Value, drawn: false));
        db.PatientTests.Add(DrawnTest(2, patient.Id.Value, drawn: false));

        var handler = new SavePatientSamplesCommandHandler(db);
        var result = await handler.Handle(new SavePatientSamplesCommand(patient.Id.Value, new[] { 1, 2 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.Equal(1, db.SaveChangesCallCount);
        Assert.True(db.PatientTests[0].IsSampleDrawn);
        Assert.True(db.PatientTests[1].IsSampleDrawn);
    }

    [Fact]
    public async Task UnmarksRemovedRows()
    {
        var db = new FakeApplicationDbContext();
        var patient = ActivePatient();
        db.Patients.Add(patient);
        db.PatientTests.Add(DrawnTest(1, patient.Id.Value, drawn: true));

        var handler = new SavePatientSamplesCommandHandler(db);
        var result = await handler.Handle(new SavePatientSamplesCommand(patient.Id.Value, Array.Empty<int>()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.False(db.PatientTests[0].IsSampleDrawn);
        Assert.Null(db.PatientTests[0].SampleDrawnAtUtc);
    }

    [Fact]
    public async Task NoDiff_NoSaveChanges_ReturnsZero()
    {
        var db = new FakeApplicationDbContext();
        var patient = ActivePatient();
        db.Patients.Add(patient);
        db.PatientTests.Add(DrawnTest(1, patient.Id.Value, drawn: true));

        var handler = new SavePatientSamplesCommandHandler(db);
        var result = await handler.Handle(new SavePatientSamplesCommand(patient.Id.Value, new[] { 1 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task SoftDeletedPatient_Rejected()
    {
        var db = new FakeApplicationDbContext();
        var patient = ActivePatient();
        patient.SoftDelete();
        db.Patients.Add(patient);

        var handler = new SavePatientSamplesCommandHandler(db);
        var result = await handler.Handle(new SavePatientSamplesCommand(patient.Id.Value, Array.Empty<int>()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task UnknownIds_Ignored_OnlyRealChangesCounted()
    {
        var db = new FakeApplicationDbContext();
        var patient = ActivePatient();
        db.Patients.Add(patient);
        db.PatientTests.Add(DrawnTest(1, patient.Id.Value, drawn: false));

        var handler = new SavePatientSamplesCommandHandler(db);
        var result = await handler.Handle(new SavePatientSamplesCommand(patient.Id.Value, new[] { 1, 999 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value); // only real test 1 changed
    }

    [Fact]
    public void Requires_AddEditPatient()
    {
        var authorized = (IAuthorizedRequest)new SavePatientSamplesCommand(1, Array.Empty<int>());
        Assert.Equal("ADD_EDIT_PATIENT", authorized.RequiredPermissionCode);
    }

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<SavePatientSamplesCommand, Result<int>>(user);

        var response = await behavior.Handle(
            new SavePatientSamplesCommand(1, Array.Empty<int>()),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
    }
}
