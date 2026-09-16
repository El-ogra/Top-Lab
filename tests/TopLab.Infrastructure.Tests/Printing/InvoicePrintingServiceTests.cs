using System.Text;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Printing;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

public class InvoicePrintingServiceTests
{
    private sealed class RecordingDispatcher : IPdfPrinterDispatcher
    {
        public string? PdfPath { get; private set; }

        public string? PrinterName { get; private set; }

        public bool Throw { get; set; }

        public Task DispatchAsync(string pdfFilePath, string printerName, CancellationToken cancellationToken = default)
        {
            if (Throw)
            {
                throw new InvalidOperationException("printer offline");
            }

            PdfPath = pdfFilePath;
            PrinterName = printerName;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeLabPrintTextStore : ILabPrintTextStore
    {
        public Task<Result<LabPrintTextDto>> GetAsync(LabPrintTextScope scope, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<LabPrintTextDto>.Success(
                new LabPrintTextDto("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12)));
        }

        public Task<Result> SaveAsync(LabPrintTextScope scope, LabPrintTextDto content, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private static InvoiceDto Dto()
    {
        return new InvoiceDto(
            7,
            "أحمد محمد علي",
            "100",
            42,
            new DateTime(2026, 9, 16, 10, 0, 0, DateTimeKind.Utc),
            new List<ChargedTestDto>
            {
                new(1, "صورة دم كاملة", "CBC", "CBC", 100m)
            },
            100m,
            10m,
            40m,
            50m,
            "L.E.");
    }

    private static (InvoicePrintingService Service, RecordingDispatcher Dispatcher, ApplicationDbContext Db) Build()
    {
        var db = new ApplicationDbContext(InMemoryContextFactory.Create());
        db.Database.EnsureCreated();
        var dispatcher = new RecordingDispatcher();
        var service = new InvoicePrintingService(db, new InvoicePdfWriter(), new FakeLabPrintTextStore(), dispatcher);
        return (service, dispatcher, db);
    }

    [Fact]
    public async Task PrintInvoiceAsync_HappyPath_WritesPdfAndDispatchesToReceiptPrinter()
    {
        var (service, dispatcher, db) = Build();
        try
        {
            var result = await service.PrintInvoiceAsync(InvoicePrintEnvelope.CreateToken(Dto()), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal("Receipt", dispatcher.PrinterName);
            Assert.NotNull(dispatcher.PdfPath);
            Assert.True(File.Exists(dispatcher.PdfPath));
            var bytes = File.ReadAllBytes(dispatcher.PdfPath!);
            Assert.StartsWith("%PDF", Encoding.ASCII.GetString(bytes));
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintInvoiceAsync_MissingPrinterAssignment_ReturnsUnexpected()
    {
        var (service, dispatcher, db) = Build();
        var receipt = db.Set<PrinterAssignment>().Single(a => a.OutputType == PrinterOutputType.Receipt);
        db.Set<PrinterAssignment>().Remove(receipt);
        db.SaveChanges();
        try
        {
            var result = await service.PrintInvoiceAsync(InvoicePrintEnvelope.CreateToken(Dto()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
            Assert.Contains("لم يتم تعيين طابعة للإيصالات.", result.Error.Message);
            Assert.Null(dispatcher.PdfPath);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintInvoiceAsync_InvalidToken_ReturnsUnexpected()
    {
        var (service, dispatcher, db) = Build();
        try
        {
            var result = await service.PrintInvoiceAsync("not-json", CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
            Assert.Null(dispatcher.PdfPath);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintInvoiceAsync_PrinterDispatchFailure_ReturnsUnexpected_InsteadOfThrowing()
    {
        var (service, dispatcher, db) = Build();
        dispatcher.Throw = true;
        try
        {
            var result = await service.PrintInvoiceAsync(InvoicePrintEnvelope.CreateToken(Dto()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
            Assert.Equal("تعذر طباعة الفاتورة.", result.Error.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public void BuildTextLines_NumberedInvoice_ShowsNumberAndFrozenPrices()
    {
        var labText = new LabPrintTextDto("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12);

        var lines = InvoicePdfWriter.BuildTextLines(Dto(), labText);

        Assert.Contains("مختبر الشفاء", lines.Header);
        Assert.Contains("رقم الفاتورة: 42", lines.InvoiceTitle);
        Assert.Contains("المريض: أحمد محمد علي", lines.Patient);
        Assert.Contains("رقم المعمل: 100", lines.Patient);
        var item = Assert.Single(lines.Items);
        Assert.Equal("صورة دم كاملة", item.Description);
        Assert.Equal("100.00 L.E.", item.Price);
        Assert.Contains("إجمالي التحاليل: 100.00 L.E.", lines.Totals);
        Assert.Contains("الباقي: 50.00 L.E.", lines.Totals);
    }

    [Fact]
    public void BuildTextLines_PreviewInvoice_ShowsPreviewTitle()
    {
        var preview = Dto() with { InvoiceNumber = null, IssuedAtUtc = null };
        var labText = new LabPrintTextDto("مختبر الشفاء", string.Empty, string.Empty, string.Empty, 0);

        var lines = InvoicePdfWriter.BuildTextLines(preview, labText);

        Assert.Equal("معاينة — بدون رقم", lines.InvoiceTitle);
    }
}
