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

public class ReceiptPrintingServiceTests
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
        public LabPrintTextDto Content { get; set; } = new("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12);

        public bool Fail { get; set; }

        public Task<Result<LabPrintTextDto>> GetAsync(LabPrintTextScope scope, CancellationToken cancellationToken = default)
        {
            if (Fail)
            {
                return Task.FromResult(Result<LabPrintTextDto>.Failure(Error.Unexpected("تعذر قراءة نصوص الطباعة المحلية.")));
            }

            return Task.FromResult(Result<LabPrintTextDto>.Success(Content));
        }

        public Task<Result> SaveAsync(LabPrintTextScope scope, LabPrintTextDto content, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private const string ReceiptPrinterName = "Receipt";

    private static ReceiptDto Dto()
    {
        return new ReceiptDto(
            7,
            "أحمد محمد علي",
            "100",
            new List<ChargedTestDto>
            {
                new(1, "صورة دم كاملة", "CBC", "CBC", 100m),
                new(2, "Glucose", "GLU", "Glucose", 50m)
            },
            150m,
            15m,
            50m,
            85m,
            "L.E.");
    }

    private static (ReceiptPrintingService Service, RecordingDispatcher Dispatcher, FakeLabPrintTextStore LabText, ApplicationDbContext Db) Build()
    {
        var db = new ApplicationDbContext(InMemoryContextFactory.Create());
        db.Database.EnsureCreated();

        // ReceiptSettings (Id=1) and the Receipt PrinterAssignment come from the
        // model's HasData seed data (applied by the InMemory provider on
        // EnsureCreated) — no manual seeding needed.
        var dispatcher = new RecordingDispatcher();
        var labText = new FakeLabPrintTextStore();
        var service = new ReceiptPrintingService(db, new ReceiptPdfWriter(), labText, dispatcher);
        return (service, dispatcher, labText, db);
    }

    [Fact]
    public async Task PrintReceiptAsync_HappyPath_WritesPdfAndDispatchesToReceiptPrinter()
    {
        var (service, dispatcher, _, db) = Build();
        try
        {
            var result = await service.PrintReceiptAsync(ReceiptPrintEnvelope.CreateToken(Dto()), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(ReceiptPrinterName, dispatcher.PrinterName);
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
    public async Task PrintReceiptAsync_MissingReceiptSettings_ReturnsUnexpected()
    {
        var (service, _, _, db) = Build();
        db.Set<ReceiptSettings>().RemoveRange(db.Set<ReceiptSettings>().ToList());
        db.SaveChanges();
        try
        {
            var result = await service.PrintReceiptAsync(ReceiptPrintEnvelope.CreateToken(Dto()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
            Assert.Contains("سجل إعدادات الإيصال", result.Error.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintReceiptAsync_MissingPrinterAssignment_ReturnsUnexpected()
    {
        var (service, dispatcher, _, db) = Build();
        var receipt = db.Set<PrinterAssignment>().Single(a => a.OutputType == PrinterOutputType.Receipt);
        db.Set<PrinterAssignment>().Remove(receipt);
        db.SaveChanges();
        try
        {
            var result = await service.PrintReceiptAsync(ReceiptPrintEnvelope.CreateToken(Dto()), CancellationToken.None);

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
    public async Task PrintReceiptAsync_InvalidToken_ReturnsUnexpected()
    {
        var (service, dispatcher, _, db) = Build();
        try
        {
            var result = await service.PrintReceiptAsync("not-json", CancellationToken.None);

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
    public async Task PrintReceiptAsync_LabTextStoreFailure_Propagates()
    {
        var (service, _, labText, db) = Build();
        labText.Fail = true;
        try
        {
            var result = await service.PrintReceiptAsync(ReceiptPrintEnvelope.CreateToken(Dto()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintReceiptAsync_PrinterDispatchFailure_ReturnsUnexpected_InsteadOfThrowing()
    {
        var (service, dispatcher, _, db) = Build();
        dispatcher.Throw = true;
        try
        {
            var result = await service.PrintReceiptAsync(ReceiptPrintEnvelope.CreateToken(Dto()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
            Assert.Equal("تعذر طباعة الإيصال.", result.Error.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public void BuildTextLines_ArabicNamesFlowVerbatim_AndTotalsCarryCurrency()
    {
        var settings = ReceiptSettings.CreateDefault();
        settings.Update(1m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.Words);
        var labText = new LabPrintTextDto("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12);

        var lines = ReceiptPdfWriter.BuildTextLines(Dto(), settings, labText);

        Assert.Contains("مختبر الشفاء", lines.Header);
        Assert.Contains("المريض: أحمد محمد علي", lines.Patient);
        Assert.Contains("رقم المعمل: 100", lines.Patient);
        Assert.Equal(2, lines.Items.Count);
        Assert.Contains(lines.Items, i => i.Description == "CBC" && i.Price == "100.00 L.E.");
        Assert.Contains("إجمالي التحاليل: 150.00 L.E.", lines.Totals);
        Assert.Contains("الخصم: 15.00 L.E.", lines.Totals);
        Assert.Contains("المدفوع: 50.00 L.E.", lines.Totals);
        Assert.Contains("الباقي: 85.00 L.E.", lines.Totals);
    }

    [Fact]
    public void BuildTextLines_HideMode_OmitsItemLines()
    {
        var settings = ReceiptSettings.CreateDefault();
        settings.Update(1m, "L.E.", null, false, TestDetailDisplayMode.Hide, false, HeaderFooterMode.None);
        var labText = new LabPrintTextDto("مختبر الشفاء", string.Empty, string.Empty, "Arial", 12);

        var lines = ReceiptPdfWriter.BuildTextLines(Dto(), settings, labText);

        Assert.Empty(lines.Items);
        Assert.Equal(4, lines.Totals.Count);
    }

    [Fact]
    public void BuildTextLines_ShowWithCode_IncludesTestCodes()
    {
        var settings = ReceiptSettings.CreateDefault();
        settings.Update(1m, "L.E.", new TimeOnly(18, 30), false, TestDetailDisplayMode.ShowWithCode, false, HeaderFooterMode.Words);
        var labText = new LabPrintTextDto("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12);

        var lines = ReceiptPdfWriter.BuildTextLines(Dto(), settings, labText);

        Assert.Contains(lines.Items, i => i.Description == "CBC — صورة دم كاملة");
        Assert.Equal("موعد الاستلام: 18:30", lines.Pickup);
    }
}
