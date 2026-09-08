using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.AddTestsToVisit;
using TopLab.Application.Features.PatientRegistration.Commands.CreatePatient;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class CreatePatientCommandHandlerTests
{
    private static CreatePatientCommand Valid(int? treatingDoctor = null, int? referral = null, int? condition = null)
    {
        return new CreatePatientCommand(
            FullName: "Ahmed",
            Sex: Sex.Male,
            AgeValue: 30,
            AgeUnit: AgeUnit.Year,
            RegistrationDateUtc: DateTime.UtcNow,
            AccountType: AccountType.Individual,
            IsVip: false,
            LabId: null,
            Title: null,
            NationalId: null,
            Address: null,
            TreatingDoctorId: treatingDoctor,
            ReferralEntityId: referral,
            PickupDateUtc: null,
            IsFastingIndicated: false,
            FastingHours: null,
            RecentContrastImaging: false,
            Notes: null,
            PhoneNumbers: new[] { new PatientNumberInput("01012345678", 0) },
            MedicalConditionIds: condition.HasValue ? new[] { condition.Value } : Array.Empty<int>(),
            Tests: new[] { new AddTestInput(10, false, false, true, false, false, false) });
    }

    private static (FakeApplicationDbContext Db, FakeSender Sender) Build()
    {
        var db = new FakeApplicationDbContext();
        var sender = new FakeSender();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.Tests.Add(Test.Create(TestId.Create(10), "CBC", "CBC", "CBC", "T10", 30, 100m));
        db.Tests.Add(Test.Create(TestId.Create(11), "Glucose", "Glucose", "Glucose", "T11", 30, 50m));
        sender.WithResponse(new GetSystemSettingsQuery(), Result<SystemSettingsDto>.Success(new SystemSettingsDto(
            AccountType.Individual, false, false, false, false, false, false, false, false,
            ResultScreenAccountDisplayMode.Hidden, false, null)));
        return (db, sender);
    }

    [Fact]
    public async Task Create_HappyPath_ReturnsPatientId_AndPersists()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var result = await handler.Handle(Valid(), CancellationToken.None);

        if (!result.IsSuccess)
        {
            throw new Xunit.Sdk.XunitException($"Result was not success. Type: {result.Error?.Type}, Message: {result.Error?.Message ?? "<null>"}");
        }
        Assert.True(result.Value >= 0);
        Assert.Single(db.Patients);
        Assert.Single(db.Patients[0].PhoneNumbers);
        Assert.Single(db.PatientTests);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Create_MissingName_Validation()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var cmd = Valid() with { FullName = "" };
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task Create_NegativeAge_Validation()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var cmd = Valid() with { AgeValue = -1 };
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task Create_FastingHoursWithoutFasting_Validation()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var cmd = Valid() with { IsFastingIndicated = false, FastingHours = 8 };
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task Create_UnknownTreatingDoctor_NotFound()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var result = await handler.Handle(Valid(treatingDoctor: 99), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Create_UnknownReferralEntity_NotFound()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var result = await handler.Handle(Valid(referral: 99), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Create_TreatingDoctorAsReferral_Validation()
    {
        var (db, sender) = Build();
        db.ExternalEntities.Add(ExternalEntity.Create(ExternalEntityId.Create(7), EntityType.TreatingDoctor, "Dr. A"));
        var handler = new CreatePatientCommandHandler(db, sender);

        var result = await handler.Handle(Valid(referral: 7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task Create_PhoneNumbers_PersistsSorted()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var cmd = Valid() with
        {
            PhoneNumbers = new[]
            {
                new PatientNumberInput("  01011111111  ", 1),
                new PatientNumberInput("01022222222", 0),
                new PatientNumberInput("", 2)
            }
        };
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, db.Patients[0].PhoneNumbers.Count);
    }

    [Fact]
    public async Task Create_UnknownCondition_NotFound()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var result = await handler.Handle(Valid(condition: 99), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Create_EmptyTests_Validation_D1()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var cmd = Valid() with { Tests = Array.Empty<AddTestInput>() };
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Empty(db.Patients);
        Assert.Empty(db.PatientTests);
    }

    [Fact]
    public async Task Create_DuplicateTests_Validation_D1()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var cmd = Valid() with
        {
            Tests = new[]
            {
                new AddTestInput(10, false, false, true, false, false, false),
                new AddTestInput(10, false, false, true, false, false, false)
            }
        };
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Empty(db.Patients);
    }

    [Fact]
    public async Task Create_UnknownTest_NotFound_D1()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var cmd = Valid() with
        {
            Tests = new[] { new AddTestInput(999, false, false, true, false, false, false) }
        };
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Empty(db.Patients);
    }

    [Fact]
    public async Task Create_MultipleTests_Persisted_InSingleSave_D1()
    {
        var (db, sender) = Build();
        var handler = new CreatePatientCommandHandler(db, sender);

        var cmd = Valid() with
        {
            Tests = new[]
            {
                new AddTestInput(10, false, false, true, false, false, false),
                new AddTestInput(11, true, false, false, false, false, false)
            }
        };
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.Patients);
        Assert.Equal(2, db.PatientTests.Count);
        Assert.Equal(1, db.SaveChangesCallCount);
        Assert.Contains(db.PatientTests, pt => pt.PriceAtOrderTime == 100m);
        Assert.Contains(db.PatientTests, pt => pt.PriceAtOrderTime == 50m);
    }
}