using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.ClearPatientAccount;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientBilling;

/// <summary>
/// R1 Round 3 B5: clear-account (canonical reference 1.3.2.7, «تصفير الحساب»).
/// Every non-voided operation of the patient is voided through the existing mechanism:
/// nothing is hard-deleted, other patients are untouched, already-voided rows stay as they
/// are, and the command is idempotent.
/// </summary>
public class ClearPatientAccountCommandHandlerTests
{
    private const int PatientIdValue = 1;

    private static FakeApplicationDbContext SeedPatient(int id = PatientIdValue, bool deleted = false)
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(PatientId.Create(id), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        if (deleted)
        {
            patient.SoftDelete();
        }

        db.Patients.Add(patient);
        return db;
    }

    private static PaymentOperation AddOperation(
        FakeApplicationDbContext db, decimal amount, int patientId = PatientIdValue, bool voided = false)
    {
        var operation = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(patientId), amount, 7, DateTime.UtcNow);
        if (voided)
        {
            operation.Void();
        }

        db.PaymentOperations.Add(operation);
        return operation;
    }

    [Fact]
    public async Task Success_VoidsEveryNonVoidedOperationOfThePatient()
    {
        var db = SeedPatient();
        var first = AddOperation(db, 100m);
        var second = AddOperation(db, 50m);

        var result = await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.True(first.IsVoided);
        Assert.True(second.IsVoided);
    }

    [Fact]
    public async Task RowsSurvive_TheAuditTrailIsNotDeleted()
    {
        var db = SeedPatient();
        AddOperation(db, 100m);

        await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.Single(db.PaymentOperations);
    }

    [Fact]
    public async Task AlreadyVoidedOperations_AreLeftAloneAndNotCounted()
    {
        var db = SeedPatient();
        AddOperation(db, 100m);
        var alreadyVoided = AddOperation(db, 50m, voided: true);

        var result = await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.Equal(1, result.Value);
        Assert.True(alreadyVoided.IsVoided);
    }

    [Fact]
    public async Task OtherPatientsOperations_AreUntouched()
    {
        var db = SeedPatient();
        AddOperation(db, 100m);
        var other = AddOperation(db, 500m, patientId: 2);

        var result = await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.Equal(1, result.Value);
        Assert.False(other.IsVoided);
    }

    [Fact]
    public async Task Idempotent_WhenNothingRemains_StillSucceedsWithZero()
    {
        var db = SeedPatient();
        AddOperation(db, 100m);
        var handler = new ClearPatientAccountCommandHandler(db);

        var first = await handler.Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);
        var second = await handler.Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.Equal(1, first.Value);
        Assert.True(second.IsSuccess);
        Assert.Equal(0, second.Value);
    }

    [Fact]
    public async Task EmptyAccount_SucceedsWithoutSaving()
    {
        var db = SeedPatient();

        var result = await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task UnknownPatient_IsNotFound()
    {
        var db = SeedPatient();

        var result = await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task SoftDeletedPatient_IsNotFound()
    {
        var db = SeedPatient(deleted: true);

        var result = await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public void ClearPatientAccount_IsNotGated_TheReferenceAddsNoPermissionCode()
    {
        Assert.IsNotAssignableFrom<TopLab.Application.Common.Authorization.IAuthorizedRequest>(
            new ClearPatientAccountCommand(1));
    }

    [Fact]
    public async Task AfterClearing_TheBalanceIsTheFullChargedAmount()
    {
        var db = SeedPatient();
        var prices = new List<decimal> { 200m };
        var operation = AddOperation(db, 100m);

        await new ClearPatientAccountCommandHandler(db)
            .Handle(new ClearPatientAccountCommand(PatientIdValue), CancellationToken.None);

        Assert.Equal(200m, PatientAccountCalculator.Balance(prices, new[] { operation }));
    }
}
