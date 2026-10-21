using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Domain.Tests.Patients;

/// <summary>
/// STAGE-01 (baseline 1.2.2.2.6 card number, 1.2.2.1.2 receipt date,
/// 1.2.2.1.1 entry date, 1.2.2.3.10 imaging question).
/// </summary>
public class PatientRegistrationFieldTests
{
    private static Patient Create() =>
        Patient.Create(PatientId.Create(1), "Ahmed Mohamed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);

    [Fact]
    public void Create_WithCardNumber_StoresTrimmedValue()
    {
        var p = Patient.Create(
            PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow,
            cardNumber: "  CARD-001  ");

        Assert.Equal("CARD-001", p.CardNumber);
    }

    [Fact]
    public void Create_BlankCardNumber_StoresNull()
    {
        var p = Patient.Create(
            PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow,
            cardNumber: "   ");

        Assert.Null(p.CardNumber);
    }

    [Fact]
    public void Create_CardNumberLongerThan30_Throws()
    {
        var tooLong = new string('C', 31);

        var ex = Assert.Throws<ArgumentException>(() => Patient.Create(
            PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow,
            cardNumber: tooLong));

        Assert.Equal("cardNumber", ex.ParamName);
    }

    [Fact]
    public void Create_CardNumberOf30Characters_IsAccepted()
    {
        var p = Patient.Create(
            PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow,
            cardNumber: new string('C', 30));

        Assert.Equal(30, p.CardNumber!.Length);
    }

    [Fact]
    public void Update_SetsCardNumberAndUltrasoundFlag()
    {
        var p = Create();

        p.Update(
            "Ahmed Mohamed", Sex.Male, 30, AgeUnit.Year, null, null, null, false,
            AccountType.Individual, null, false, null, false,
            cardNumber: "CARD-9", recentUltrasoundImaging: true);

        Assert.Equal("CARD-9", p.CardNumber);
        Assert.True(p.RecentUltrasoundImaging);
    }

    [Fact]
    public void Update_CardNumberLongerThan30_Throws()
    {
        var p = Create();

        var ex = Assert.Throws<ArgumentException>(() => p.Update(
            "Ahmed Mohamed", Sex.Male, 30, AgeUnit.Year, null, null, null, false,
            AccountType.Individual, null, false, null, false,
            cardNumber: new string('C', 31)));

        Assert.Equal("cardNumber", ex.ParamName);
    }

    [Fact]
    public void Update_SetsPickupAndRegistrationDates()
    {
        var p = Create();
        var pickup = new DateTime(2026, 3, 1, 10, 30, 0, DateTimeKind.Utc);
        var registration = new DateTime(2026, 2, 28, 9, 0, 0, DateTimeKind.Utc);

        p.Update(
            "Ahmed Mohamed", Sex.Male, 30, AgeUnit.Year, null, null, null, false,
            AccountType.Individual, null, false, null, false,
            pickupDateUtc: pickup, registrationDateUtc: registration);

        Assert.Equal(pickup, p.PickupDateUtc);
        Assert.Equal(registration, p.RegistrationDateUtc);
    }

    [Fact]
    public void Update_WithoutDates_KeepsTheStoredOnes()
    {
        var created = new DateTime(2026, 2, 28, 9, 0, 0, DateTimeKind.Utc);
        var p = Patient.Create(
            PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, created,
            pickupDateUtc: created);

        p.Update(
            "Ahmed Mohamed", Sex.Male, 30, AgeUnit.Year, null, null, null, false,
            AccountType.Individual, null, false, null, false);

        Assert.Equal(created, p.RegistrationDateUtc);
        Assert.Equal(created, p.PickupDateUtc);
    }

    [Fact]
    public void HasRecentImaging_TrueWhenEitherFlagIsSet()
    {
        var contrastOnly = Patient.Create(
            PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow,
            recentContrastImaging: true);
        var ultrasoundOnly = Patient.Create(
            PatientId.Create(2), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow,
            recentUltrasoundImaging: true);
        var none = Patient.Create(
            PatientId.Create(3), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);

        Assert.True(contrastOnly.HasRecentImaging);
        Assert.True(ultrasoundOnly.HasRecentImaging);
        Assert.False(none.HasRecentImaging);
    }
}
