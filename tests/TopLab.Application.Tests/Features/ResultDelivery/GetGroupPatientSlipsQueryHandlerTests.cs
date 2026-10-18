using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Queries.GetGroupPatientSlips;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultDelivery;

/// <summary>
/// Phase 6 (GAP-013): the group-patient report slip query.
/// Covers selected-patient results, identity + associated tests, the selection
/// mechanism (explicit id list), filtering and empty selection.
/// </summary>
public class GetGroupPatientSlipsQueryHandlerTests
{
    private static SystemSettingsDto SettingsDto(bool printLabIdInsteadOfPatientId)
    {
        return new SystemSettingsDto(
            AccountType.Individual, false, false, false, false, false,
            printLabIdInsteadOfPatientId, false, false,
            ResultScreenAccountDisplayMode.Hidden, false, null);
    }

    private static Patient MakePatient(int id, string name, string? labId, DateTime registeredUtc)
    {
        return Patient.Create(
            PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, registeredUtc,
            labId: labId is null ? null : LabId.Create(labId));
    }

    private static (FakeApplicationDbContext Db, FakeSender Sender) Build(
        bool printLabIdInsteadOfPatientId = true)
    {
        var db = new FakeApplicationDbContext();
        var sender = new FakeSender();
        sender.WithResponse(
            new GetSystemSettingsQuery(),
            Result<SystemSettingsDto>.Success(SettingsDto(printLabIdInsteadOfPatientId)));
        return (db, sender);
    }

    [Fact]
    public async Task Slips_OnePerSelectedPatient_WithIdentityAndAssociatedTests()
    {
        var (db, sender) = Build();
        db.Patients.Add(MakePatient(1, "أحمد محمد", "100", new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(2, "سيد علي", "200", new DateTime(2026, 10, 2, 8, 0, 0, DateTimeKind.Utc)));
        db.Tests.Add(Test.Create(TestId.Create(10), "صورة دم كاملة", "CBC", "CBC-R", "CBC", 30, 100m));
        db.Tests.Add(Test.Create(TestId.Create(11), "Glucose", "GLU", "GLU-R", "GLU", 30, 50m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(11), 50m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(102), PatientId.Create(2), TestId.Create(10), 100m));

        var handler = new GetGroupPatientSlipsQueryHandler(db, sender);
        var result = await handler.Handle(new GetGroupPatientSlipsQuery(new[] { 1, 2 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var batch = result.Value!;
        Assert.Equal(2, batch.Slips.Count);

        var first = batch.Slips.Single(s => s.PatientId == 1);
        Assert.Equal("أحمد محمد", first.FullName);
        Assert.Equal("100", first.LabId);
        Assert.Equal("100", first.Identifier);
        Assert.Equal(new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc), first.RegistrationDateUtc);
        Assert.Equal(2, first.Lines.Count);
        Assert.Equal("CBC", first.Lines[0].TestCode);
        Assert.Equal("صورة دم كاملة", first.Lines[0].TestName);
        Assert.Equal("GLU", first.Lines[1].TestCode);

        // Only the intended patient's tests travel with that patient's slip.
        var second = batch.Slips.Single(s => s.PatientId == 2);
        Assert.Equal("سيد علي", second.FullName);
        Assert.Equal("200", second.Identifier);
        var only = Assert.Single(second.Lines);
        Assert.Equal("CBC", only.TestCode);
    }

    [Fact]
    public async Task Slips_OrderedByRegistrationDate_ThenId()
    {
        var (db, sender) = Build();
        db.Patients.Add(MakePatient(5, "أخير", null, new DateTime(2026, 10, 3, 8, 0, 0, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(3, "أول", null, new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(4, "ثاني", null, new DateTime(2026, 10, 2, 8, 0, 0, DateTimeKind.Utc)));

        var handler = new GetGroupPatientSlipsQueryHandler(db, sender);
        var result = await handler.Handle(new GetGroupPatientSlipsQuery(new[] { 5, 3, 4 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 3, 4, 5 }, result.Value!.Slips.Select(s => s.PatientId).ToArray());
    }

    [Fact]
    public async Task Slips_FilterOutUnknownAndSoftDeletedPatients_KeepingTheRest()
    {
        var (db, sender) = Build();
        db.Patients.Add(MakePatient(1, "أحمد", null, DateTime.UtcNow));
        var deleted = MakePatient(2, "محذوف", null, DateTime.UtcNow);
        deleted.SoftDelete();
        db.Patients.Add(deleted);

        var handler = new GetGroupPatientSlipsQueryHandler(db, sender);
        var result = await handler.Handle(new GetGroupPatientSlipsQuery(new[] { 1, 2, 99 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var slip = Assert.Single(result.Value!.Slips);
        Assert.Equal(1, slip.PatientId);
    }

    [Fact]
    public async Task Slips_DeduplicateRepeatedIds_ProducingOneSlipPerPatient()
    {
        var (db, sender) = Build();
        db.Patients.Add(MakePatient(1, "أحمد", null, DateTime.UtcNow));

        var handler = new GetGroupPatientSlipsQueryHandler(db, sender);
        var result = await handler.Handle(new GetGroupPatientSlipsQuery(new[] { 1, 1, 1 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Slips);
    }

    [Fact]
    public async Task Slips_PatientWithoutTests_StillCarriesItsIdentity()
    {
        var (db, sender) = Build();
        db.Patients.Add(MakePatient(1, "أحمد", "100", DateTime.UtcNow));

        var handler = new GetGroupPatientSlipsQueryHandler(db, sender);
        var result = await handler.Handle(new GetGroupPatientSlipsQuery(new[] { 1 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var slip = Assert.Single(result.Value!.Slips);
        Assert.Empty(slip.Lines);
        Assert.Equal("أحمد", slip.FullName);
        Assert.Equal("100", slip.Identifier);
    }

    [Fact]
    public async Task Slips_FlagOff_UsesPatientIdAsIdentifier()
    {
        var (db, sender) = Build(printLabIdInsteadOfPatientId: false);
        db.Patients.Add(MakePatient(7, "أحمد", "100", DateTime.UtcNow));

        var handler = new GetGroupPatientSlipsQueryHandler(db, sender);
        var result = await handler.Handle(new GetGroupPatientSlipsQuery(new[] { 7 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("7", Assert.Single(result.Value!.Slips).Identifier);
    }

    [Fact]
    public async Task Slips_MissingLabId_IdentifierFallsBackToPatientId()
    {
        var (db, sender) = Build(printLabIdInsteadOfPatientId: true);
        db.Patients.Add(MakePatient(7, "أحمد", null, DateTime.UtcNow));

        var handler = new GetGroupPatientSlipsQueryHandler(db, sender);
        var result = await handler.Handle(new GetGroupPatientSlipsQuery(new[] { 7 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var slip = Assert.Single(result.Value!.Slips);
        Assert.Null(slip.LabId);
        Assert.Equal("7", slip.Identifier);
    }

    [Fact]
    public async Task Slips_NoMatchingPatients_ReturnsEmptyBatchNotFailure()
    {
        var (db, sender) = Build();

        var handler = new GetGroupPatientSlipsQueryHandler(db, sender);
        var result = await handler.Handle(new GetGroupPatientSlipsQuery(new[] { 42 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Slips);
    }

    [Fact]
    public async Task Slips_SettingsFailure_Propagates()
    {
        var db = new FakeApplicationDbContext();
        var sender = new FakeSender();
        sender.WithResponse(
            new GetSystemSettingsQuery(),
            Result<SystemSettingsDto>.Failure(Error.Unexpected("سجل إعدادات النظام مفقود.")));

        var handler = new GetGroupPatientSlipsQueryHandler(db, sender);
        var result = await handler.Handle(new GetGroupPatientSlipsQuery(new[] { 1 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
    }
}

public class GetGroupPatientSlipsQueryValidatorTests
{
    private readonly GetGroupPatientSlipsQueryValidator _validator = new();

    [Fact]
    public void Validate_EmptySelection_Invalid_WithFrozenMessage()
    {
        var result = _validator.Validate(new GetGroupPatientSlipsQuery(Array.Empty<int>()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "قائمة المرضى مطلوبة.");
    }

    [Fact]
    public void Validate_NonPositivePatientId_Invalid()
    {
        var result = _validator.Validate(new GetGroupPatientSlipsQuery(new[] { 0 }));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "معرّف المريض غير صالح.");
    }

    [Fact]
    public void Validate_ValidIds_Valid()
    {
        var result = _validator.Validate(new GetGroupPatientSlipsQuery(new[] { 1, 2 }));

        Assert.True(result.IsValid);
    }
}
