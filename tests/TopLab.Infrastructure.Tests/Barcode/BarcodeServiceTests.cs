using System.Text;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Barcode;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Printing;
using TopLab.Infrastructure.Tests.Common;
using TopLab.Infrastructure.Tests.Common.Fakes;
using Xunit;
using ZXing;

namespace TopLab.Infrastructure.Tests.Barcode;

public class BarcodeServiceTests
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

    private const string BarcodePrinterName = "Barcode";

    private static (BarcodeService Service, RecordingDispatcher Dispatcher, FakeDateTimeProvider Clock, ApplicationDbContext Db) Build()
    {
        var db = new ApplicationDbContext(InMemoryContextFactory.Create());
        db.Database.EnsureCreated();

        // SystemSettings (Id=1) and the Barcode PrinterAssignment come from the
        // model's HasData seed data (applied by the InMemory provider on
        // EnsureCreated) — no manual seeding needed.
        var dispatcher = new RecordingDispatcher();
        var clock = new FakeDateTimeProvider();
        var service = new BarcodeService(db, new BarcodeLabelRenderer(), clock, dispatcher);
        return (service, dispatcher, clock, db);
    }

    [Fact]
    public async Task PrintBarcodeAsync_HappyPath_WritesPdfAndDispatchesToBarcodePrinter()
    {
        var (service, dispatcher, _, db) = Build();
        try
        {
            var result = await service.PrintBarcodeAsync("12345", CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(BarcodePrinterName, dispatcher.PrinterName);
            Assert.NotNull(dispatcher.PdfPath);
            Assert.True(File.Exists(dispatcher.PdfPath));
            var bytes = File.ReadAllBytes(dispatcher.PdfPath!);
            Assert.StartsWith("%PDF", Encoding.ASCII.GetString(bytes));
            Assert.Contains("(12345)", Encoding.ASCII.GetString(bytes));
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public void RenderedLabel_ScansBackToExpectedPayload()
    {
        var renderer = new BarcodeLabelRenderer();

        var label = renderer.Render("LAB-42");

        Assert.Equal(300, label.Width);
        Assert.Equal(80, label.Height);
        Assert.NotEmpty(label.Pixels);

        var reader = new BarcodeReaderGeneric();
        var decoded = reader.Decode(label.Pixels, label.Width, label.Height, RGBLuminanceSource.BitmapFormat.RGBA32);

        Assert.NotNull(decoded);
        Assert.Equal("LAB-42", decoded!.Text);
        Assert.Equal(BarcodeFormat.CODE_128, decoded.BarcodeFormat);
    }

    [Fact]
    public async Task PrintBarcodeAsync_DateTimeFlagOff_PayloadIsIdentifierOnly()
    {
        var (service, dispatcher, _, db) = Build();
        try
        {
            var result = await service.PrintBarcodeAsync("777", CancellationToken.None);

            Assert.True(result.IsSuccess);
            var text = Encoding.ASCII.GetString(File.ReadAllBytes(dispatcher.PdfPath!));
            Assert.Contains("(777)", text);
            Assert.DoesNotContain("2026", text);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintBarcodeAsync_DateTimeFlagOn_PayloadAppendsPrintDateTime()
    {
        var (service, dispatcher, clock, db) = Build();
        clock.UtcNow = new DateTime(2026, 5, 4, 13, 45, 0, DateTimeKind.Utc);
        var settings = db.Set<SystemSettings>().Single(s => s.Id == 1);
        settings.SetGeneralFlags(false, false, false, false, true, false, false, false);
        db.SaveChanges();
        try
        {
            var result = await service.PrintBarcodeAsync("777", CancellationToken.None);

            Assert.True(result.IsSuccess);
            var text = Encoding.ASCII.GetString(File.ReadAllBytes(dispatcher.PdfPath!));
            Assert.Contains("(777 2026-05-04 13:45)", text);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintBarcodeAsync_MissingBarcodePrinterAssignment_ReturnsUnexpected()
    {
        var (service, dispatcher, _, db) = Build();
        var barcode = db.Set<PrinterAssignment>().Single(a => a.OutputType == PrinterOutputType.Barcode);
        db.Set<PrinterAssignment>().Remove(barcode);
        db.SaveChanges();
        try
        {
            var result = await service.PrintBarcodeAsync("12345", CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
            Assert.Contains("لم يتم تعيين طابعة للباركود.", result.Error.Message);
            Assert.Null(dispatcher.PdfPath);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintBarcodeAsync_MissingSystemSettings_ReturnsUnexpected()
    {
        var (service, _, _, db) = Build();
        db.Set<SystemSettings>().RemoveRange(db.Set<SystemSettings>().ToList());
        db.SaveChanges();
        try
        {
            var result = await service.PrintBarcodeAsync("12345", CancellationToken.None);

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
    public async Task PrintBarcodeAsync_BlankValue_ReturnsUnexpected()
    {
        var (service, dispatcher, _, db) = Build();
        try
        {
            var result = await service.PrintBarcodeAsync("  ", CancellationToken.None);

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
    public async Task PrintBarcodeAsync_PrinterDispatchFailure_ReturnsUnexpected_InsteadOfThrowing()
    {
        var (service, dispatcher, _, db) = Build();
        dispatcher.Throw = true;
        try
        {
            var result = await service.PrintBarcodeAsync("12345", CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
            Assert.Equal("تعذر طباعة الباركود.", result.Error.Message);
        }
        finally
        {
            db.Dispose();
        }
    }
}
