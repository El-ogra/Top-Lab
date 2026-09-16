using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientInvoice;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientBilling.Queries;

public class GetPatientInvoiceQueryHandlerTests
{
    private static FakeApplicationDbContext BuildDb()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(7), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        db.Tests.Add(Test.Create(TestId.Create(10), "CBC", "CBC", "CBC", "T10", 30, 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(7), TestId.Create(10), 100m));
        db.ReceiptSettings.Add(ReceiptSettings.CreateDefault());
        return db;
    }

    [Fact]
    public async Task GetInvoice_NoIssueYet_PreviewWithoutNumber()
    {
        var db = BuildDb();
        var handler = new GetPatientInvoiceQueryHandler(db);

        var result = await handler.Handle(new GetPatientInvoiceQuery(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.InvoiceNumber);
        Assert.Null(result.Value.IssuedAtUtc);
        Assert.Equal(100m, result.Value.TotalCharged);
        Assert.Equal("L.E.", result.Value.Currency);
        Assert.Single(result.Value.ChargedTests);
    }

    [Fact]
    public async Task GetInvoice_WithIssues_ReturnsLatestNumber()
    {
        var db = BuildDb();
        var atUtc = new DateTime(2026, 9, 16, 10, 0, 0, DateTimeKind.Utc);
        db.InvoiceIssues.Add(InvoiceIssue.Create(InvoiceIssueId.Create(1), PatientId.Create(7), 1, atUtc, 3, 100m, 0m, 1));
        db.InvoiceIssues.Add(InvoiceIssue.Create(InvoiceIssueId.Create(2), PatientId.Create(7), 2, atUtc, 3, 100m, 0m, 1));
        var handler = new GetPatientInvoiceQueryHandler(db);

        var result = await handler.Handle(new GetPatientInvoiceQuery(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.InvoiceNumber);
        Assert.Equal(atUtc, result.Value.IssuedAtUtc);
    }

    [Fact]
    public async Task GetInvoice_TotalsMatchAccountQuery()
    {
        var db = BuildDb();
        var invoice = await new GetPatientInvoiceQueryHandler(db).Handle(new GetPatientInvoiceQuery(7), CancellationToken.None);
        var account = await new GetPatientAccountQueryHandler(db).Handle(new GetPatientAccountQuery(7), CancellationToken.None);

        Assert.True(invoice.IsSuccess);
        Assert.True(account.IsSuccess);
        Assert.Equal(account.Value!.TotalCharged, invoice.Value!.TotalCharged);
        Assert.Equal(account.Value.TotalDiscount, invoice.Value.TotalDiscount);
        Assert.Equal(account.Value.TotalPaid, invoice.Value.TotalPaid);
        Assert.Equal(account.Value.Balance, invoice.Value.Balance);
    }

    [Fact]
    public async Task GetInvoice_UnknownPatient_NotFound()
    {
        var db = BuildDb();
        var handler = new GetPatientInvoiceQueryHandler(db);

        var result = await handler.Handle(new GetPatientInvoiceQuery(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}

public class GetPatientInvoiceQueryValidatorTests
{
    private readonly GetPatientInvoiceQueryValidator _validator = new();

    [Fact]
    public void Validate_ZeroPatientId_Invalid()
    {
        var result = _validator.Validate(new GetPatientInvoiceQuery(0));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_PositivePatientId_Valid()
    {
        var result = _validator.Validate(new GetPatientInvoiceQuery(7));

        Assert.True(result.IsValid);
    }
}
