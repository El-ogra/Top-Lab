using System.Text;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Barcode;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Printing;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// Phase 6 (GAP-013): the group-slip printing service — token round-trip, lab-text
/// scope, printer routing and the failure surface.
/// </summary>
public class GroupPatientSlipPrintingServiceTests
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

        public LabPrintTextScope? LastScope { get; private set; }

        public Task<Result<LabPrintTextDto>> GetAsync(LabPrintTextScope scope, CancellationToken cancellationToken = default)
        {
            LastScope = scope;
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

    private static GroupPatientSlipBatchDto Batch(int patients)
    {
        var slips = new List<GroupPatientSlipDto>();
        for (var i = 1; i <= patients; i++)
        {
            slips.Add(new GroupPatientSlipDto(
                i,
                $"مريض {i}",
                $"10{i}",
                $"10{i}",
                new DateTime(2026, 10, 1, 9, 30, 0, DateTimeKind.Utc),
                new List<GroupPatientSlipLineDto> { new("CBC", "صورة دم كاملة") }));
        }

        return new GroupPatientSlipBatchDto(slips);
    }

    private static (GroupPatientSlipPrintingService Service, RecordingDispatcher Dispatcher, FakeLabPrintTextStore LabText, ApplicationDbContext Db) Build()
    {
        var db = new ApplicationDbContext(InMemoryContextFactory.Create());
        db.Database.EnsureCreated();

        // SystemSettings and the Reports PrinterAssignment come from the model's
        // HasData seed data (applied by the InMemory provider on EnsureCreated).
        var dispatcher = new RecordingDispatcher();
        var labText = new FakeLabPrintTextStore();
        var service = new GroupPatientSlipPrintingService(
            db, new GroupPatientSlipPdfWriter(new BarcodeLabelRenderer()), labText, dispatcher);
        return (service, dispatcher, labText, db);
    }

    [Fact]
    public async Task PrintAsync_HappyPath_WritesPdfAndDispatchesToReportsPrinter()
    {
        var (service, dispatcher, labText, db) = Build();
        try
        {
            var result = await service.PrintGroupPatientSlipsAsync(
                GroupPatientSlipPrintEnvelope.CreateToken(Batch(2)), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(dispatcher.PrinterName);
            Assert.NotNull(dispatcher.PdfPath);
            Assert.True(File.Exists(dispatcher.PdfPath));
            var bytes = File.ReadAllBytes(dispatcher.PdfPath!);
            Assert.StartsWith("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));

            // The follow-up/delivery desk uses the Receipt lab-text block,
            // matching the lab-order slip's decision 68-B.
            Assert.Equal(LabPrintTextScope.Receipt, labText.LastScope);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintAsync_EmptyBatch_StillProducesAValidDocument()
    {
        var (service, dispatcher, _, db) = Build();
        try
        {
            var result = await service.PrintGroupPatientSlipsAsync(
                GroupPatientSlipPrintEnvelope.CreateToken(Batch(0)), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(File.Exists(dispatcher.PdfPath));
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintAsync_InvalidToken_ReturnsUnexpected()
    {
        var (service, _, _, db) = Build();
        try
        {
            var result = await service.PrintGroupPatientSlipsAsync("not-json", CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("بيانات قصاصات المرضى غير صالحة.", result.Error!.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintAsync_MissingSystemSettings_ReturnsUnexpected()
    {
        var (service, _, _, db) = Build();
        try
        {
            db.Set<SystemSettings>().RemoveRange(db.Set<SystemSettings>().ToList());
            db.SaveChanges();

            var result = await service.PrintGroupPatientSlipsAsync(
                GroupPatientSlipPrintEnvelope.CreateToken(Batch(1)), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("سجل إعدادات النظام مفقود.", result.Error!.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintAsync_MissingReportsAssignment_ReturnsUnexpected()
    {
        var (service, _, _, db) = Build();
        try
        {
            var assignment = db.Set<PrinterAssignment>().FirstOrDefault(a => a.OutputType == PrinterOutputType.Reports);
            Assert.NotNull(assignment);
            db.Set<PrinterAssignment>().Remove(assignment);
            db.SaveChanges();

            var result = await service.PrintGroupPatientSlipsAsync(
                GroupPatientSlipPrintEnvelope.CreateToken(Batch(1)), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("لم يتم تعيين طابعة لتقارير المختبر.", result.Error!.Message);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintAsync_LabTextFailure_Propagates()
    {
        var (service, _, labText, db) = Build();
        try
        {
            labText.Fail = true;

            var result = await service.PrintGroupPatientSlipsAsync(
                GroupPatientSlipPrintEnvelope.CreateToken(Batch(1)), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Fact]
    public async Task PrintAsync_DispatcherFailure_MapsToUnexpected()
    {
        var (service, dispatcher, _, db) = Build();
        try
        {
            dispatcher.Throw = true;

            var result = await service.PrintGroupPatientSlipsAsync(
                GroupPatientSlipPrintEnvelope.CreateToken(Batch(1)), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("تعذر طباعة قصاصات المرضى.", result.Error!.Message);
        }
        finally
        {
            db.Dispose();
        }
    }
}
