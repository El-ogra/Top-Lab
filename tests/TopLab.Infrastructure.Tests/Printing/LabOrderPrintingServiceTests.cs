using System.Text;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Barcode;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Printing;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

public class LabOrderPrintingServiceTests
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

            // Decision 68-B: the slip uses the Receipt lab-text block.
            Assert.Equal(LabPrintTextScope.Receipt, scope);
            return Task.FromResult(Result<LabPrintTextDto>.Success(Content));
        }

        public Task<Result> SaveAsync(LabPrintTextScope scope, LabPrintTextDto content, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private static LabOrderDto Order()
    {
        return new LabOrderDto(
            7,
            "أحمد محمد علي",
            "100",
            "100",
            new DateTime(2026, 10, 1, 9, 30, 0, DateTimeKind.Utc),
            new List<LabOrderLineDto> { new("CBC", "صورة دم كاملة") });
    }

    private static (LabOrderPrintingService Service, RecordingDispatcher Dispatcher, FakeLabPrintTextStore LabText, ApplicationDbContext Db) Build()
    {
        var db = new ApplicationDbContext(InMemoryContextFactory.Create());
        db.Database.EnsureCreated();

        // SystemSettings, Receipt lab-text scope aside, and the Reports
        // PrinterAssignment come from the model's HasData seed data (applied by
        // the InMemory provider on EnsureCreated) — no manual seeding needed.
        var dispatcher = new RecordingDispatcher();
        var labText = new FakeLabPrintTextStore();
        var service = new LabOrderPrintingService(db, new LabOrderPdfWriter(new BarcodeLabelRenderer()), labText, dispatcher);
        return (service, dispatcher, labText, db);
    }

    [Fact]
    public async Task PrintLabOrderAsync_HappyPath_WritesPdfAndDispatchesToReportsPrinter()
    {
        var (service, dispatcher, _, db) = Build();
        try
        {
            var result = await service.PrintLabOrderAsync(LabOrderPrintEnvelope.CreateToken(Order()), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(dispatcher.PrinterName);
            Assert.NotNull(dispatcher.PdfPath);
            Assert.True(File.Exists(dispatcher.PdfPath));
            var bytes = File.ReadAllBytes(dispatcher.PdfPath!);
            Assert.StartsWith("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintLabOrderAsync_InvalidToken_ReturnsUnexpected()
    {
        var (service, _, _, db) = Build();
        try
        {
            var result = await service.PrintLabOrderAsync("not-json", CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("بيانات طلب التحاليل غير صالحة.", result.Error!.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintLabOrderAsync_MissingSystemSettings_ReturnsUnexpected()
    {
        var (service, _, _, db) = Build();
        try
        {
            db.Set<SystemSettings>().RemoveRange(db.Set<SystemSettings>().ToList());
            db.SaveChanges();

            var result = await service.PrintLabOrderAsync(LabOrderPrintEnvelope.CreateToken(Order()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("سجل إعدادات النظام مفقود.", result.Error!.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintLabOrderAsync_MissingReportsAssignment_ReturnsUnexpected()
    {
        var (service, _, _, db) = Build();
        try
        {
            var assignment = db.Set<PrinterAssignment>().FirstOrDefault(a => a.OutputType == PrinterOutputType.Reports);
            Assert.NotNull(assignment);
            db.Set<PrinterAssignment>().Remove(assignment);
            db.SaveChanges();

            var result = await service.PrintLabOrderAsync(LabOrderPrintEnvelope.CreateToken(Order()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("لم يتم تعيين طابعة لتقارير المختبر.", result.Error!.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintLabOrderAsync_LabTextFailure_Propagates()
    {
        var (service, _, labText, db) = Build();
        try
        {
            labText.Fail = true;

            var result = await service.PrintLabOrderAsync(LabOrderPrintEnvelope.CreateToken(Order()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintLabOrderAsync_DispatcherFailure_MapsToUnexpected()
    {
        var (service, dispatcher, _, db) = Build();
        try
        {
            dispatcher.Throw = true;

            var result = await service.PrintLabOrderAsync(LabOrderPrintEnvelope.CreateToken(Order()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("تعذر طباعة طلب التحاليل.", result.Error!.Message);
        }
        finally
        {
            db.Dispose();
        }
    }
}
