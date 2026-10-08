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

public class EnvelopePrintingServiceTests
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

            Assert.Equal(LabPrintTextScope.Envelope, scope);
            return Task.FromResult(Result<LabPrintTextDto>.Success(Content));
        }

        public Task<Result> SaveAsync(LabPrintTextScope scope, LabPrintTextDto content, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private static EnvelopeDto Dto()
    {
        return new EnvelopeDto(
            7,
            "أحمد محمد علي",
            "100",
            "د. أحمد",
            new DateTime(2026, 10, 1, 9, 30, 0, DateTimeKind.Utc),
            "100");
    }

    private static (EnvelopePrintingService Service, RecordingDispatcher Dispatcher, FakeLabPrintTextStore LabText, ApplicationDbContext Db) Build()
    {
        var db = new ApplicationDbContext(InMemoryContextFactory.Create());
        db.Database.EnsureCreated();

        // EnvelopeSettings (Id=1), the four EnvelopePrintItemPositions, and the
        // Envelope PrinterAssignment come from the model's HasData seed data
        // (applied by the InMemory provider on EnsureCreated) — no manual
        // seeding needed.
        var dispatcher = new RecordingDispatcher();
        var labText = new FakeLabPrintTextStore();
        var service = new EnvelopePrintingService(db, new EnvelopePdfWriter(new BarcodeLabelRenderer()), labText, dispatcher);
        return (service, dispatcher, labText, db);
    }

    [Fact]
    public async Task PrintEnvelopeAsync_HappyPath_WritesPdfAndDispatchesToEnvelopePrinter()
    {
        var (service, dispatcher, _, db) = Build();
        try
        {
            var result = await service.PrintEnvelopeAsync(EnvelopePrintEnvelope.CreateToken(Dto()), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal("Envelope", dispatcher.PrinterName);
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
    public async Task PrintEnvelopeAsync_InvalidToken_ReturnsUnexpected()
    {
        var (service, _, _, db) = Build();
        try
        {
            var result = await service.PrintEnvelopeAsync("not-json", CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
            Assert.Equal("بيانات المظروف غير صالحة.", result.Error.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintEnvelopeAsync_MissingEnvelopeSettings_ReturnsUnexpected()
    {
        var (service, _, _, db) = Build();
        try
        {
            db.Set<EnvelopeSettings>().RemoveRange(db.Set<EnvelopeSettings>().ToList());
            db.SaveChanges();

            var result = await service.PrintEnvelopeAsync(EnvelopePrintEnvelope.CreateToken(Dto()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("سجل إعدادات المظروف مفقود.", result.Error!.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintEnvelopeAsync_MissingPrinterAssignment_ReturnsUnexpected()
    {
        var (service, _, _, db) = Build();
        try
        {
            var assignment = db.Set<PrinterAssignment>().FirstOrDefault(a => a.OutputType == PrinterOutputType.Envelope);
            Assert.NotNull(assignment);
            db.Set<PrinterAssignment>().Remove(assignment);
            db.SaveChanges();

            var result = await service.PrintEnvelopeAsync(EnvelopePrintEnvelope.CreateToken(Dto()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("لم يتم تعيين طابعة للمظاريف.", result.Error!.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintEnvelopeAsync_LabTextFailure_Propagates()
    {
        var (service, _, labText, db) = Build();
        try
        {
            labText.Fail = true;

            var result = await service.PrintEnvelopeAsync(EnvelopePrintEnvelope.CreateToken(Dto()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintEnvelopeAsync_DispatcherFailure_MapsToUnexpected()
    {
        var (service, dispatcher, _, db) = Build();
        try
        {
            dispatcher.Throw = true;

            var result = await service.PrintEnvelopeAsync(EnvelopePrintEnvelope.CreateToken(Dto()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("تعذر طباعة المظروف.", result.Error!.Message);
        }
        finally
        {
            db.Dispose();
        }
    }
}
