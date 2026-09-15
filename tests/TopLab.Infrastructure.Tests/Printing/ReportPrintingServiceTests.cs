using System.Text;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Printing;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

public class ReportPrintingServiceTests
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

    private const string PrinterName = "Reports";

    private static ApplicationDbContext BuildDb()
    {
        var db = new ApplicationDbContext(InMemoryContextFactory.Create());
        db.Database.EnsureCreated();

        // ReportSettings (Id=1), SystemSettings (Id=1) and the Reports
        // PrinterAssignment come from the model's HasData seed data (applied by
        // the InMemory provider on EnsureCreated) — no manual seeding needed.
        return db;
    }

    private static string Token()
    {
        var dto = new CombinedReportDto(
            7,
            "Ali",
            "LAB-1",
            new List<CombinedReportLineDto>
            {
                new(101, 2, "Glucose", "GLU", 0, "5.5", 0, "4-6", new List<ProfileReportLineDto>(), null)
            });

        return ReportPrintEnvelope.CreateToken(ReportPrintEnvelope.Combined, dto);
    }

    private static (ReportPrintingService Service, RecordingDispatcher Dispatcher, ApplicationDbContext Db) Build()
    {
        var db = BuildDb();
        var dispatcher = new RecordingDispatcher();
        var service = new ReportPrintingService(db, new ReportPdfWriter(), dispatcher);
        return (service, dispatcher, db);
    }

    [Fact]
    public async Task PrintReportAsync_HappyPath_WritesPdfAndDispatchesToAssignedPrinter()
    {
        var (service, dispatcher, db) = Build();
        try
        {
            var result = await service.PrintReportAsync(Token(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(PrinterName, dispatcher.PrinterName);
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
    public async Task PrintReportAsync_MissingPrinterAssignment_ReturnsUnexpected()
    {
        var db = BuildDb();
        db.Set<PrinterAssignment>().RemoveRange(db.Set<PrinterAssignment>().ToList());
        db.SaveChanges();
        try
        {
            var dispatcher = new RecordingDispatcher();
            var service = new ReportPrintingService(db, new ReportPdfWriter(), dispatcher);

            var result = await service.PrintReportAsync(Token(), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
            Assert.Contains("طابعة", result.Error.Message);
            Assert.Null(dispatcher.PdfPath);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintReportAsync_MissingReportSettings_ReturnsUnexpected()
    {
        var db = BuildDb();
        db.Set<ReportSettings>().RemoveRange(db.Set<ReportSettings>().ToList());
        db.SaveChanges();
        try
        {
            var service = new ReportPrintingService(db, new ReportPdfWriter(), new RecordingDispatcher());

            var result = await service.PrintReportAsync(Token(), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
            Assert.Contains("سجل إعدادات التقرير", result.Error.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintReportAsync_MissingSystemSettings_ReturnsUnexpected()
    {
        var db = BuildDb();
        db.Set<SystemSettings>().RemoveRange(db.Set<SystemSettings>().ToList());
        db.SaveChanges();
        try
        {
            var service = new ReportPrintingService(db, new ReportPdfWriter(), new RecordingDispatcher());

            var result = await service.PrintReportAsync(Token(), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
            Assert.Contains("سجل إعدادات النظام", result.Error.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintReportAsync_InvalidToken_ReturnsUnexpected()
    {
        var (service, dispatcher, db) = Build();
        try
        {
            var result = await service.PrintReportAsync("not-json", CancellationToken.None);

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
    public async Task PrintReportAsync_PrinterDispatchFailure_ReturnsUnexpected_InsteadOfThrowing()
    {
        var (service, dispatcher, db) = Build();
        dispatcher.Throw = true;
        try
        {
            var result = await service.PrintReportAsync(Token(), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintReportAsync_SettingsReflectedAtCallTime_TwoCallsProduceDistinctOutputs()
    {
        var (service, dispatcher, db) = Build();
        try
        {
            var first = await service.PrintReportAsync(Token(), CancellationToken.None);
            Assert.True(first.IsSuccess);
            var firstPath = dispatcher.PdfPath!;

            var settings = db.Set<SystemSettings>().Single(s => s.Id == 1);
            settings.SetGeneralFlags(false, false, false, false, false, true, false, false);
            db.SaveChanges();

            var second = await service.PrintReportAsync(Token(), CancellationToken.None);
            Assert.True(second.IsSuccess);
            var secondPath = dispatcher.PdfPath!;

            Assert.NotEqual(firstPath, secondPath);
            var firstText = Encoding.ASCII.GetString(File.ReadAllBytes(firstPath));
            var secondText = Encoding.ASCII.GetString(File.ReadAllBytes(secondPath));
            Assert.Contains("PatientId: 7", firstText);
            Assert.DoesNotContain("LabId:", firstText);
            Assert.Contains("LabId: LAB-1", secondText);
            Assert.DoesNotContain("PatientId:", secondText);
        }
        finally
        {
            db.Dispose();
        }
    }
}