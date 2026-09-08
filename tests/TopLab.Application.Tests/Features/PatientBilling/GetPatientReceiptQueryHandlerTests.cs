using TopLab.Application.Features.PatientBilling.Queries.GetPatientReceipt;
using TopLab.Application.Common.Results;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientBilling;

public class GetPatientReceiptQueryHandlerTests
{
    private static Patient MakePatient(int id)
    {
        return Patient.Create(PatientId.Create(id), "Mona", Sex.Female, 25, AgeUnit.Year, DateTime.UtcNow);
    }

    [Fact]
    public async Task HappyPath_UsesCurrencyFromSettings_AndReceiptNames()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        db.Tests.Add(Test.Create(TestId.Create(10), "CBC", "CBC", "CBC-RECEIPT", "T10", 30, 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));
        db.PaymentOperations.Add(PaymentOperation.Create(
            PaymentOperationId.Create(1), PatientId.Create(1), 60m, 7, DateTime.UtcNow, 5m));
        var settings = ReceiptSettings.CreateDefault();
        settings.Update(1.0m, "EGP", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.None);
        db.ReceiptSettings.Add(settings);

        var handler = new GetPatientReceiptQueryHandler(db);
        var result = await handler.Handle(new GetPatientReceiptQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var receipt = result.Value!;
        Assert.Equal(1, receipt.PatientId);
        Assert.Equal("Mona", receipt.PatientFullName);
        Assert.Equal("EGP", receipt.Currency);
        Assert.Single(receipt.ChargedTests);
        Assert.Equal("CBC-RECEIPT", receipt.ChargedTests[0].ReceiptName);
        Assert.Equal(100m, receipt.TotalCharged);
        Assert.Equal(5m, receipt.TotalDiscount);
        Assert.Equal(65m, receipt.TotalPaid);
        Assert.Equal(35m, receipt.Balance);
    }

    [Fact]
    public async Task MissingSettingsRow_ReturnsUnexpected()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));

        var handler = new GetPatientReceiptQueryHandler(db);
        var result = await handler.Handle(new GetPatientReceiptQuery(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
        Assert.Equal("سجل إعدادات الإيصال مفقود.", result.Error!.Message);
    }

    [Fact]
    public async Task UnknownPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        db.ReceiptSettings.Add(ReceiptSettings.CreateDefault());

        var handler = new GetPatientReceiptQueryHandler(db);
        var result = await handler.Handle(new GetPatientReceiptQuery(77), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task SoftDeletedPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var patient = MakePatient(1);
        patient.SoftDelete();
        db.Patients.Add(patient);
        db.ReceiptSettings.Add(ReceiptSettings.CreateDefault());

        var handler = new GetPatientReceiptQueryHandler(db);
        var result = await handler.Handle(new GetPatientReceiptQuery(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }
}
