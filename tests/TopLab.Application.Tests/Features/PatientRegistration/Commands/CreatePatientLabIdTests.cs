using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.AddTestsToVisit;
using TopLab.Application.Features.PatientRegistration.Commands.CreatePatient;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class CreatePatientLabIdTests
{
    private static CreatePatientCommand Valid(string? labId)
    {
        return new CreatePatientCommand(
            FullName: "Ahmed",
            Sex: Sex.Male,
            AgeValue: 30,
            AgeUnit: AgeUnit.Year,
            RegistrationDateUtc: DateTime.UtcNow,
            AccountType: AccountType.Individual,
            IsVip: false,
            LabId: labId,
            Title: null,
            NationalId: null,
            Address: null,
            TreatingDoctorId: null,
            ReferralEntityId: null,
            PickupDateUtc: null,
            IsFastingIndicated: false,
            FastingHours: null,
            RecentContrastImaging: false,
            Notes: null,
            PhoneNumbers: new[] { new PatientNumberInput("01012345678", 0) },
            MedicalConditionIds: Array.Empty<int>(),
            Tests: new[] { new AddTestInput(10, false, false, true, false, false, false) });
    }

    private static (FakeApplicationDbContext Db, FakeSender Sender) Build()
    {
        var db = new FakeApplicationDbContext();
        var sender = new FakeSender();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.Tests.Add(Test.Create(TestId.Create(10), "CBC", "CBC", "CBC", "T10", 30, 100m));
        sender.WithResponse(new GetSystemSettingsQuery(), Result<SystemSettingsDto>.Success(new SystemSettingsDto(
            AccountType.Individual, false, false, false, false, false, false, false, false,
            ResultScreenAccountDisplayMode.Hidden, false, null)));
        return (db, sender);
    }

    [Fact]
    public async Task Create_ManualLabId_PersistsVerbatim()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var result = await handler.Handle(Valid("100"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("100", Assert.Single(db.Patients).LabId!.Value);
    }

    [Fact]
    public async Task Create_DuplicateLabId_Conflict()
    {
        var (db, sender) = Build();
        db.Patients.Add(Patient.Create(
            PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow,
            labId: LabId.Create("100")));
        var handler = new CreatePatientCommandHandler(db, sender);

        var result = await handler.Handle(Valid("100"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("رمز المعمل مستخدم بالفعل.", result.Error.Message);
        Assert.Single(db.Patients);
    }

    [Fact]
    public async Task Create_DuplicateLabId_BlockedEvenAfterSoftDelete()
    {
        var (db, sender) = Build();
        var existing = Patient.Create(
            PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow,
            labId: LabId.Create("100"));
        existing.SoftDelete();
        db.Patients.Add(existing);
        var handler = new CreatePatientCommandHandler(db, sender);

        var result = await handler.Handle(Valid("100"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public async Task Create_EmptyLabId_TreatedAsAbsent()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var result = await handler.Handle(Valid("  "), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(Assert.Single(db.Patients).LabId);
    }

    [Fact]
    public async Task Create_DistinctLabIds_BothSucceed()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var first = await handler.Handle(Valid("100"), CancellationToken.None);
        var second = await handler.Handle(Valid("101"), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(2, db.Patients.Count);
    }
}
