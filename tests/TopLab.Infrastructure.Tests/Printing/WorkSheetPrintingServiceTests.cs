using System.Text;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Printing;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

public class WorkSheetPrintingServiceTests
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
            Assert.Equal(LabPrintTextScope.Report, scope);
            return Task.FromResult(Result<LabPrintTextDto>.Success(
                new LabPrintTextDto("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12)));
        }

        public Task<Result> SaveAsync(LabPrintTextScope scope, LabPrintTextDto content, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private static VisitWorkSheetDto Sheet()
    {
        return new VisitWorkSheetDto(
            7,
            "أحمد محمد علي",
            "100",
            new List<WorkSheetSectionDto>
            {
                new(5, "G5", new List<WorkSheetLineDto>
                {
                    new(100, 7, "أحمد محمد علي", "100", "صورة دم كاملة", "CBC", "BC-10", true, null, false, false, 60),
                    new(101, 7, "أحمد محمد علي", "100", "Glucose", "GLU", null, false, null, false, false, 30)
                })
            },
            new List<VisitWorkSheetSampleDto>
            {
                new(100, true, false, true, false, false, false),
                new(101, false, false, false, false, false, true)
            },
            2,
            false,
            false,
            true);
    }

    private static (WorkSheetPrintingService Service, RecordingDispatcher Dispatcher, ApplicationDbContext Db) Build()
    {
        var db = new ApplicationDbContext(InMemoryContextFactory.Create());
        db.Database.EnsureCreated();
        var dispatcher = new RecordingDispatcher();
        var service = new WorkSheetPrintingService(db, new WorkSheetPdfWriter(), new FakeLabPrintTextStore(), dispatcher);
        return (service, dispatcher, db);
    }

    [Fact]
    public async Task PrintWorkSheetAsync_HappyPath_WritesPdfAndDispatchesToReportsPrinter()
    {
        var (service, dispatcher, db) = Build();
        try
        {
            var result = await service.PrintWorkSheetAsync(WorkSheetPrintEnvelope.CreateToken(Sheet()), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal("Reports", dispatcher.PrinterName);
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
    public async Task PrintWorkSheetAsync_MissingReportsPrinterAssignment_ReturnsUnexpected()
    {
        var (service, dispatcher, db) = Build();
        var reports = db.Set<PrinterAssignment>().Single(a => a.OutputType == PrinterOutputType.Reports);
        db.Set<PrinterAssignment>().Remove(reports);
        db.SaveChanges();
        try
        {
            var result = await service.PrintWorkSheetAsync(WorkSheetPrintEnvelope.CreateToken(Sheet()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
            Assert.Contains("لم يتم تعيين طابعة لتقارير المختبر.", result.Error.Message);
            Assert.Null(dispatcher.PdfPath);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintWorkSheetAsync_InvalidToken_ReturnsUnexpected()
    {
        var (service, dispatcher, db) = Build();
        try
        {
            var result = await service.PrintWorkSheetAsync("not-json", CancellationToken.None);

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
    public async Task PrintWorkSheetAsync_PrinterDispatchFailure_ReturnsUnexpected_InsteadOfThrowing()
    {
        var (service, dispatcher, db) = Build();
        dispatcher.Throw = true;
        try
        {
            var result = await service.PrintWorkSheetAsync(WorkSheetPrintEnvelope.CreateToken(Sheet()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
            Assert.Equal("تعذر طباعة ورقة العمل.", result.Error.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public void BuildTextLines_MixedFlags_MapsLettersAndBarcodes()
    {
        var labText = new LabPrintTextDto("مختبر الشفاء", "شارع الجمهورية", "01000000000", "Arial", 12);

        var lines = WorkSheetPdfWriter.BuildTextLines(Sheet(), labText);

        Assert.Contains("مختبر الشفاء", lines.Header);
        Assert.Contains("المريض: أحمد محمد علي", lines.Patient);
        Assert.Contains("رقم المعمل: 100", lines.Patient);
        var section = Assert.Single(lines.Sections);
        Assert.Equal("G5", section.Name);
        Assert.Equal(2, section.Rows.Count);
        Assert.Equal("BC-10", section.Rows[0].Barcode);
        Assert.Equal("U B", section.Rows[0].SampleKinds);
        Assert.Equal("[X]", section.Rows[0].Drawn);
        Assert.Equal("—", section.Rows[1].Barcode);
        Assert.Equal("خارج", section.Rows[1].SampleKinds);
        Assert.Equal("[ ]", section.Rows[1].Drawn);
        Assert.Equal("إجمالي التحاليل: 2", lines.TotalLine);
    }

    [Fact]
    public void SampleKinds_AllFlags_JoinsLetters()
    {
        var kinds = WorkSheetPdfWriter.SampleKinds(new VisitWorkSheetSampleDto(1, true, true, true, true, true, false));

        Assert.Equal("U S B Se CSF", kinds);
    }
}
