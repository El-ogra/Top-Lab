using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.PrintInvoice;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientBilling.Commands;

public class PrintInvoiceCommandHandlerTests
{
    private sealed class CapturingInvoicePrintingService : IInvoicePrintingService
    {
        public string? LastToken { get; private set; }

        public Result? NextResult { get; set; }

        public Task<Result> PrintInvoiceAsync(string invoiceToken, CancellationToken cancellationToken = default)
        {
            LastToken = invoiceToken;
            return Task.FromResult(NextResult ?? Result.Success());
        }
    }

    /// <summary>
    /// Wrapper that throws a SQL-Server-shaped unique violation on the first
    /// save only, proving the handler's retry-once path (M-12 idiom).
    /// </summary>
    private sealed class ThrowOnceUniqueViolationDb : IApplicationDbContext
    {
        private readonly FakeApplicationDbContext _inner = new();

        public FakeApplicationDbContext Inner => _inner;

        public IQueryable<TEntity> Set<TEntity>() where TEntity : class => _inner.Set<TEntity>();

        public void Add<TEntity>(TEntity entity) where TEntity : class => _inner.Add(entity);

        public void Update<TEntity>(TEntity entity) where TEntity : class => _inner.Update(entity);

        public void Remove<TEntity>(TEntity entity) where TEntity : class => _inner.Remove(entity);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_inner.SaveChangesCallCount == 0)
            {
                _inner.SaveChangesAsync(cancellationToken).GetAwaiter().GetResult();
                throw new Exception(
                    "Violation of UNIQUE KEY constraint 'IX_InvoiceIssues_InvoiceNumber'. Cannot insert duplicate key in object 'dbo.InvoiceIssues'.");
            }

            return _inner.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
            => _inner.CanConnectAsync(cancellationToken);
    }

    private static FakeApplicationDbContext BuildDb()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(7), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        db.Tests.Add(Test.Create(TestId.Create(10), "CBC", "CBC", "CBC", "T10", 30, 100m));
        db.Tests.Add(Test.Create(TestId.Create(11), "Glucose", "Glucose", "Glucose", "T11", 30, 50m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(7), TestId.Create(10), 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(7), TestId.Create(11), 50m));
        db.ReceiptSettings.Add(ReceiptSettings.CreateDefault());
        return db;
    }

    [Fact]
    public async Task PrintInvoice_TwoPrints_AllocateSequentialNumbers()
    {
        var db = BuildDb();
        var printing = new CapturingInvoicePrintingService();
        var handler = new PrintInvoiceCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider(), printing);

        var first = await handler.Handle(new PrintInvoiceCommand(7), CancellationToken.None);
        var second = await handler.Handle(new PrintInvoiceCommand(7), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(1, first.Value);
        Assert.Equal(2, second.Value);
        Assert.Equal(2, db.InvoiceIssues.Count);
    }

    [Fact]
    public async Task PrintInvoice_UniqueViolation_RetriesOnceWithFreshNumber()
    {
        var wrapper = new ThrowOnceUniqueViolationDb();
        var db = wrapper.Inner;
        db.Patients.Add(Patient.Create(PatientId.Create(7), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        db.ReceiptSettings.Add(ReceiptSettings.CreateDefault());
        var printing = new CapturingInvoicePrintingService();
        var handler = new PrintInvoiceCommandHandler(wrapper, new FakeCurrentUserService(), new FakeDateTimeProvider(), printing);

        var result = await handler.Handle(new PrintInvoiceCommand(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.Single(db.InvoiceIssues);
    }

    [Fact]
    public async Task PrintInvoice_UnknownPatient_NotFound()
    {
        var db = BuildDb();
        var printing = new CapturingInvoicePrintingService();
        var handler = new PrintInvoiceCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider(), printing);

        var result = await handler.Handle(new PrintInvoiceCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error.Message);
        Assert.Null(printing.LastToken);
    }

    [Fact]
    public async Task PrintInvoice_ItemLines_CarryFrozenPrices()
    {
        var db = BuildDb();
        var printing = new CapturingInvoicePrintingService();
        var handler = new PrintInvoiceCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider(), printing);

        var result = await handler.Handle(new PrintInvoiceCommand(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var issue = Assert.Single(db.InvoiceIssues);
        Assert.Equal(150m, issue.TotalCharged);
        Assert.Equal(2, issue.ItemCount);
        Assert.NotNull(printing.LastToken);
        var envelope = System.Text.Json.JsonSerializer.Deserialize<InvoicePrintEnvelope>(printing.LastToken!);
        var dto = System.Text.Json.JsonSerializer.Deserialize<InvoiceDto>(envelope!.InvoiceJson);
        Assert.NotNull(dto);
        Assert.Equal(new[] { 100m, 50m }, dto!.ChargedTests.Select(t => t.PriceAtOrderTime).ToArray());
        Assert.Equal(150m, dto.TotalCharged);
    }

    [Fact]
    public async Task PrintInvoice_PrintingFailure_Propagates()
    {
        var db = BuildDb();
        var printing = new CapturingInvoicePrintingService
        {
            NextResult = Result.Failure(Error.Unexpected("تعذر طباعة الفاتورة."))
        };
        var handler = new PrintInvoiceCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider(), printing);

        var result = await handler.Handle(new PrintInvoiceCommand(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
    }
}

public class PrintInvoiceCommandValidatorTests
{
    private readonly PrintInvoiceCommandValidator _validator = new();

    [Fact]
    public void Validate_ZeroPatientId_Invalid()
    {
        var result = _validator.Validate(new PrintInvoiceCommand(0));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_PositivePatientId_Valid()
    {
        var result = _validator.Validate(new PrintInvoiceCommand(7));

        Assert.True(result.IsValid);
    }
}
