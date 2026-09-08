using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

public class BalanceProbeTests
{
    private static Patient MakePatient(int id)
    {
        return Patient.Create(PatientId.Create(id), $"P{id}", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
    }

    [Fact]
    public void Balance_DelegatesTo_DomainCalculator_WorkedExample()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(11), PatientId.Create(1), TestId.Create(1), 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(12), PatientId.Create(1), TestId.Create(2), 50m));

        var extra = PaymentOperation.Create(PaymentOperationId.Create(21), PatientId.Create(1), 20m, 1, DateTime.UtcNow, null, true);
        var pay = PaymentOperation.Create(PaymentOperationId.Create(22), PatientId.Create(1), 80m, 1, DateTime.UtcNow, 10m);
        var voided = PaymentOperation.Create(PaymentOperationId.Create(23), PatientId.Create(1), 999m, 1, DateTime.UtcNow);
        voided.Void();
        db.PaymentOperations.Add(extra);
        db.PaymentOperations.Add(pay);
        db.PaymentOperations.Add(voided);

        var probe = BalanceProbe.Balance(db, 1);

        var prices = new List<decimal> { 100m, 50m };
        var ops = new List<PaymentOperation> { extra, pay, voided };
        Assert.Equal(PatientAccountCalculator.Balance(prices, ops), probe);
        Assert.Equal(80m, probe);
    }

    [Fact]
    public void Balance_Empty_ReturnsZero()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(9));
        Assert.Equal(0m, BalanceProbe.Balance(db, 9));
    }
}
