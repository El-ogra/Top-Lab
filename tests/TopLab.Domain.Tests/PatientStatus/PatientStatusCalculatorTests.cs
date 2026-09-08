using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.PatientStatus;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Domain.Tests.PatientStatus;

public class PatientStatusCalculatorTests
{
    private static int _nextId = 5000;

    private static Patient NewPatient(DateTime registrationDateUtc)
    {
        return Patient.Create(
            PatientId.Create(_nextId++),
            "Status Patient",
            Sex.Male,
            30,
            AgeUnit.Year,
            registrationDateUtc);
    }

    private static PatientTest EntryPending()
    {
        return PatientTest.Create(PatientTestId.Create(_nextId++), PatientId.Create(1), TestId.Create(1), 100m);
    }

    private static PatientTest ReviewPending()
    {
        var pt = PatientTest.Create(PatientTestId.Create(_nextId++), PatientId.Create(1), TestId.Create(1), 100m);
        pt.EnterResult("5.0", ResultFlag.Normal, 1, DateTime.UtcNow);
        return pt;
    }

    private static PatientTest PrintPending()
    {
        var pt = ReviewPending();
        pt.MarkReviewed(1, DateTime.UtcNow);
        return pt;
    }

    private static PatientTest DeliveryPending()
    {
        var pt = PrintPending();
        pt.MarkPrinted(1, DateTime.UtcNow);
        return pt;
    }

    private static PatientTest Delivered()
    {
        var pt = DeliveryPending();
        pt.MarkDelivered(1, DateTime.UtcNow);
        return pt;
    }

    private readonly PatientStatusCalculator _sut = new();

    [Fact]
    public void S1_WhenNewlyRegistered_And_NoResultEntered()
    {
        var patient = NewPatient(DateTime.UtcNow);
        var tests = new List<PatientTest> { EntryPending(), EntryPending() };

        Assert.Equal(PatientAggregateStatus.S1, _sut.Calculate(patient, tests, 0m));
    }

    [Fact]
    public void S2_WhenRegistrationNotToday_And_NoResultEntered()
    {
        var patient = NewPatient(DateTime.UtcNow.AddDays(-1));
        var tests = new List<PatientTest> { EntryPending() };

        Assert.Equal(PatientAggregateStatus.S2, _sut.Calculate(patient, tests, 0m));
    }

    [Fact]
    public void S2_WhenMixedEnteredAndUnentered()
    {
        var patient = NewPatient(DateTime.UtcNow);
        var tests = new List<PatientTest> { EntryPending(), ReviewPending() };

        Assert.Equal(PatientAggregateStatus.S2, _sut.Calculate(patient, tests, 0m));
    }

    [Fact]
    public void BindingWorkedExample_Stages_1_3_4_MapsTo_S2()
    {
        var patient = NewPatient(DateTime.UtcNow.AddDays(-1));
        var tests = new List<PatientTest>
        {
            PrintPending(),
            DeliveryPending(),
            EntryPending(),
        };

        Assert.Equal(PatientAggregateStatus.S2, _sut.Calculate(patient, tests, 0m));
    }

    [Fact]
    public void S3_WhenReviewPending_And_NoEntryPending()
    {
        var patient = NewPatient(DateTime.UtcNow.AddDays(-5));
        var tests = new List<PatientTest> { ReviewPending(), PrintPending() };

        Assert.Equal(PatientAggregateStatus.S3, _sut.Calculate(patient, tests, 0m));
    }

    [Fact]
    public void S4_WhenPrintPending_And_NoEarlierStage()
    {
        var patient = NewPatient(DateTime.UtcNow.AddDays(-5));
        var tests = new List<PatientTest> { PrintPending(), DeliveryPending() };

        Assert.Equal(PatientAggregateStatus.S4, _sut.Calculate(patient, tests, 0m));
    }

    [Fact]
    public void S5_WhenDeliveryPending()
    {
        var patient = NewPatient(DateTime.UtcNow.AddDays(-5));
        var tests = new List<PatientTest> { DeliveryPending(), Delivered() };

        Assert.Equal(PatientAggregateStatus.S5, _sut.Calculate(patient, tests, 0m));
    }

    [Fact]
    public void S5_TakesPrecedence_Over_Balance()
    {
        var patient = NewPatient(DateTime.UtcNow.AddDays(-5));
        var tests = new List<PatientTest> { DeliveryPending() };

        Assert.Equal(PatientAggregateStatus.S5, _sut.Calculate(patient, tests, 150m));
    }

    [Fact]
    public void S6_OnlyWhen_AllDelivered_WithBalance()
    {
        var patient = NewPatient(DateTime.UtcNow.AddDays(-5));
        var tests = new List<PatientTest> { Delivered(), Delivered() };

        Assert.Equal(PatientAggregateStatus.S6, _sut.Calculate(patient, tests, 80m));
    }

    [Fact]
    public void S7_When_AllDelivered_And_Settled()
    {
        var patient = NewPatient(DateTime.UtcNow.AddDays(-5));
        var tests = new List<PatientTest> { Delivered() };

        Assert.Equal(PatientAggregateStatus.S7, _sut.Calculate(patient, tests, 0m));
    }

    [Fact]
    public void S7_When_AllDelivered_And_Credit()
    {
        var patient = NewPatient(DateTime.UtcNow.AddDays(-5));
        var tests = new List<PatientTest> { Delivered() };

        Assert.Equal(PatientAggregateStatus.S7, _sut.Calculate(patient, tests, -10m));
    }

    [Fact]
    public void S1_S2_Boundary_RegistrationToday_Vs_Yesterday()
    {
        var today = NewPatient(DateTime.UtcNow);
        var yesterday = NewPatient(DateTime.UtcNow.AddDays(-1));

        Assert.Equal(PatientAggregateStatus.S1, _sut.Calculate(today, new List<PatientTest> { EntryPending() }, 0m));
        Assert.Equal(PatientAggregateStatus.S2, _sut.Calculate(yesterday, new List<PatientTest> { EntryPending() }, 0m));
    }

    [Fact]
    public void MinOverStages_Picks_Earliest_Incomplete()
    {
        var patient = NewPatient(DateTime.UtcNow.AddDays(-3));
        var tests = new List<PatientTest> { Delivered(), ReviewPending(), Delivered() };

        Assert.Equal(PatientAggregateStatus.S3, _sut.Calculate(patient, tests, 999m));
    }
}
