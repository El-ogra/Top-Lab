using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.UpdatePatient;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class UpdatePatientCommandHandlerTests
{
    [Fact]
    public async Task Update_HappyPath_Persists()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "Old", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.SetPhoneNumbers(new[] { new PatientNumberInput("010", 0) });
        db.Patients.Add(p);

        var handler = new UpdatePatientCommandHandler(db);
        var cmd = new UpdatePatientCommand(
            PatientId: 1,
            FullName: "New",
            Sex: Sex.Female,
            AgeValue: 25,
            AgeUnit: AgeUnit.Year,
            NationalId: null,
            Address: null,
            Title: null,
            IsVip: true,
            AccountType: AccountType.Individual,
            Notes: null,
            IsFastingIndicated: false,
            FastingHours: null,
            RecentContrastImaging: false,
            PhoneNumbers: new[]
            {
                new PatientNumberInput("999", 0),
                new PatientNumberInput("888", 1)
            });

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New", p.FullName);
        Assert.Equal(2, p.PhoneNumbers.Count);
    }

    [Fact]
    public async Task Update_UnknownPatient_NotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdatePatientCommandHandler(db);

        var cmd = new UpdatePatientCommand(999, "X", Sex.Male, 30, AgeUnit.Year, null, null, null, false, AccountType.Individual, null, false, null, false, Array.Empty<PatientNumberInput>());

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Update_SoftDeletedPatient_Conflict()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.SoftDelete();
        db.Patients.Add(p);

        var handler = new UpdatePatientCommandHandler(db);
        var cmd = new UpdatePatientCommand(1, "Y", Sex.Male, 30, AgeUnit.Year, null, null, null, false, AccountType.Individual, null, false, null, false, Array.Empty<PatientNumberInput>());

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public async Task Update_ReplacesPhoneNumbers_NotAppends()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.SetPhoneNumbers(new[] { new PatientNumberInput("111", 0), new PatientNumberInput("222", 1) });
        db.Patients.Add(p);

        var handler = new UpdatePatientCommandHandler(db);
        var cmd = new UpdatePatientCommand(1, "X", Sex.Male, 30, AgeUnit.Year, null, null, null, false, AccountType.Individual, null, false, null, false, new[]
        {
            new PatientNumberInput("999", 0)
        });

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(p.PhoneNumbers);
    }
}