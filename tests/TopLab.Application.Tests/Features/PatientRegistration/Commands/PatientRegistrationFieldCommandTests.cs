using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.AddTestsToVisit;
using TopLab.Application.Features.PatientRegistration.Commands.CreatePatient;
using TopLab.Application.Features.PatientRegistration.Commands.UpdatePatient;
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

/// <summary>
/// STAGE-01: the create / update patient commands carry the card number, the
/// receipt date, the corrected registration date and the ultrasound flag.
/// </summary>
public class PatientRegistrationFieldCommandTests
{
    private static CreatePatientCommand Valid() => new(
        FullName: "Ahmed",
        Sex: Sex.Male,
        AgeValue: 30,
        AgeUnit: AgeUnit.Year,
        RegistrationDateUtc: new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc),
        AccountType: AccountType.Individual,
        IsVip: false,
        LabId: null,
        Title: null,
        NationalId: null,
        Address: null,
        TreatingDoctorId: null,
        ReferralEntityId: null,
        PickupDateUtc: new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc),
        IsFastingIndicated: false,
        FastingHours: null,
        RecentContrastImaging: true,
        Notes: null,
        PhoneNumbers: Array.Empty<PatientNumberInput>(),
        MedicalConditionIds: Array.Empty<int>(),
        Tests: new[] { new AddTestInput(10, false, false, true, false, false, false) },
        CardNumber: "CARD-42",
        RecentUltrasoundImaging: true);

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
    public async Task Create_PersistsCardNumberPickupDateAndUltrasoundFlag()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var result = await handler.Handle(Valid(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var patient = Assert.Single(db.Patients);
        Assert.Equal("CARD-42", patient.CardNumber);
        Assert.Equal(new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc), patient.PickupDateUtc);
        Assert.Equal(new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc), patient.RegistrationDateUtc);
        Assert.True(patient.RecentContrastImaging);
        Assert.True(patient.RecentUltrasoundImaging);
        Assert.True(patient.HasRecentImaging);
    }

    [Fact]
    public async Task Create_CardNumberLongerThan30_Validation()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var cmd = Valid() with { CardNumber = new string('C', 31) };
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Empty(db.Patients);
    }

    [Fact]
    public async Task Update_PersistsCardNumberDatesAndUltrasoundFlag()
    {
        var (db, _) = Build();
        var patient = Patient.Create(
            PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year,
            new DateTime(2026, 2, 28, 8, 0, 0, DateTimeKind.Utc));
        db.Patients.Add(patient);

        var handler = new UpdatePatientCommandHandler(db);
        var cmd = new UpdatePatientCommand(
            PatientId: 1,
            FullName: "Ahmed Mohamed",
            Sex: Sex.Female,
            AgeValue: 31,
            AgeUnit: AgeUnit.Year,
            NationalId: null,
            Address: null,
            Title: null,
            IsVip: false,
            AccountType: AccountType.Individual,
            Notes: null,
            IsFastingIndicated: false,
            FastingHours: null,
            RecentContrastImaging: false,
            PhoneNumbers: Array.Empty<PatientNumberInput>(),
            CardNumber: "CARD-7",
            RecentUltrasoundImaging: true,
            PickupDateUtc: new DateTime(2026, 3, 2, 11, 15, 0, DateTimeKind.Utc),
            RegistrationDateUtc: new DateTime(2026, 2, 28, 7, 45, 0, DateTimeKind.Utc));

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("CARD-7", patient.CardNumber);
        Assert.Equal(new DateTime(2026, 3, 2, 11, 15, 0, DateTimeKind.Utc), patient.PickupDateUtc);
        Assert.Equal(new DateTime(2026, 2, 28, 7, 45, 0, DateTimeKind.Utc), patient.RegistrationDateUtc);
        Assert.True(patient.RecentUltrasoundImaging);
        Assert.False(patient.RecentContrastImaging);
    }

    [Fact]
    public async Task Update_CardNumberLongerThan30_Validation()
    {
        var (db, _) = Build();
        var patient = Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        db.Patients.Add(patient);

        var handler = new UpdatePatientCommandHandler(db);
        var cmd = new UpdatePatientCommand(
            1, "Ahmed", Sex.Male, 30, AgeUnit.Year, null, null, null, false,
            AccountType.Individual, null, false, null, false, Array.Empty<PatientNumberInput>(),
            CardNumber: new string('C', 31));

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task Update_WithoutDates_KeepsTheStoredDates()
    {
        var (db, _) = Build();
        var registered = new DateTime(2026, 2, 28, 8, 0, 0, DateTimeKind.Utc);
        var patient = Patient.Create(
            PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, registered,
            pickupDateUtc: registered);
        db.Patients.Add(patient);

        var handler = new UpdatePatientCommandHandler(db);
        var cmd = new UpdatePatientCommand(
            1, "Ahmed", Sex.Male, 30, AgeUnit.Year, null, null, null, false,
            AccountType.Individual, null, false, null, false, Array.Empty<PatientNumberInput>());

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(registered, patient.RegistrationDateUtc);
        Assert.Equal(registered, patient.PickupDateUtc);
    }
}
