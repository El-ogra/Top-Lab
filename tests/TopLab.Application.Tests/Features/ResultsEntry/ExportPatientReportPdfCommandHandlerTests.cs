using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;
using TopLab.Application.Features.ResultsEntry.Commands.ClearResult;
using TopLab.Application.Features.ResultsEntry.Commands.EnterResult;
using TopLab.Application.Features.ResultsEntry.Commands.ExportPatientReportPdf;
using TopLab.Application.Features.ResultsEntry.Commands.MarkAllPatientResultsReviewed;
using TopLab.Application.Features.ResultsEntry.Commands.MarkResultDelivered;
using TopLab.Application.Features.ResultsEntry.Commands.MarkResultPrinted;
using TopLab.Application.Features.ResultsEntry.Commands.RefreshResultReferenceRange;
using TopLab.Application.Features.ResultsEntry.Commands.ReviewResult;
using TopLab.Application.Features.ResultsEntry.Commands.UnreviewResult;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

public sealed class FakeReportPdfExporter : IPatientReportPdfExporter
{
    public int CallCount { get; private set; }

    public bool ThrowOnExport { get; set; }

    public PatientReportPdfData? LastData { get; private set; }

    public string? LastPath { get; private set; }

    public Task ExportAsync(string absolutePath, PatientReportPdfData data, CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastPath = absolutePath;
        LastData = data;
        if (ThrowOnExport)
        {
            throw new IOException("simulated I/O failure");
        }

        return Task.CompletedTask;
    }
}

public class ExportPatientReportPdfCommandHandlerTests
{
    private static Patient MakePatient(int id = 1)
    {
        return Patient.Create(PatientId.Create(id), $"P{id}", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
    }

    private static void AddSimpleTest(FakeApplicationDbContext db, int id)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), $"T{id}", $"T{id}", $"T{id}", $"T{id}", 30, 100m, ResultKind.Simple));
    }

    private static PatientTest VerifiedRow(int ptId, int patientId, int testId)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m);
        pt.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        return pt;
    }

    private static string NewTempPdfPath()
    {
        return Path.Combine(Path.GetTempPath(), $"toplab-export-{Guid.NewGuid():N}.pdf");
    }

    [Fact]
    public async Task Success_Marks_Exported_Without_PrintCount_Or_Lifecycle_Change()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        var row = VerifiedRow(101, 1, 10);
        db.PatientTests.Add(row);
        var exporter = new FakeReportPdfExporter();
        var path = NewTempPdfPath();

        try
        {
            var handler = new ExportPatientReportPdfCommandHandler(db, exporter, new FakeDateTimeProvider());
            var result = await handler.Handle(new ExportPatientReportPdfCommand(1, path), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(1, exporter.CallCount);
            Assert.True(row.IsExported);
            Assert.NotNull(row.ExportedAtUtc);
            Assert.False(row.IsPrinted);
            Assert.Equal(0, row.PrintCount);
            Assert.True(row.IsReviewed);
            Assert.False(row.IsDelivered);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task ExistingFile_Returns_Conflict_And_NeverOverwrites()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        db.PatientTests.Add(VerifiedRow(101, 1, 10));
        var exporter = new FakeReportPdfExporter();
        var path = Path.GetTempFileName();
        var pdfPath = Path.ChangeExtension(path, ".pdf");
        File.Move(path, pdfPath);
        var before = File.ReadAllBytes(pdfPath);

        try
        {
            var handler = new ExportPatientReportPdfCommandHandler(db, exporter, new FakeDateTimeProvider());
            var result = await handler.Handle(new ExportPatientReportPdfCommand(1, pdfPath), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("Conflict", result.Error!.Code);
            Assert.Equal(0, exporter.CallCount);
            Assert.False(db.PatientTests[0].IsExported);
            Assert.Equal(before, File.ReadAllBytes(pdfPath));
        }
        finally
        {
            if (File.Exists(pdfPath))
            {
                File.Delete(pdfPath);
            }
        }
    }

    [Theory]
    [InlineData("relative\\report.pdf")]
    [InlineData("report.pdf")]
    public async Task NonAbsolutePath_Validation(string relative)
    {
        var db = new FakeApplicationDbContext();
        var exporter = new FakeReportPdfExporter();
        var handler = new ExportPatientReportPdfCommandHandler(db, exporter, new FakeDateTimeProvider());
        var result = await handler.Handle(new ExportPatientReportPdfCommand(1, relative), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Validation", result.Error!.Code);
        Assert.Equal(0, exporter.CallCount);
    }

    [Fact]
    public async Task NonPdfPath_Validation()
    {
        var absolute = Path.Combine(Path.GetTempPath(), "report.txt");
        var db = new FakeApplicationDbContext();
        var exporter = new FakeReportPdfExporter();
        var handler = new ExportPatientReportPdfCommandHandler(db, exporter, new FakeDateTimeProvider());
        var result = await handler.Handle(new ExportPatientReportPdfCommand(1, absolute), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Validation", result.Error!.Code);
        Assert.Equal(0, exporter.CallCount);
    }

    [Fact]
    public async Task UnverifiedReport_Conflict_Uses_Print_Eligibility()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        var pt = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        pt.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow);
        db.PatientTests.Add(pt);
        var exporter = new FakeReportPdfExporter();

        var handler = new ExportPatientReportPdfCommandHandler(db, exporter, new FakeDateTimeProvider());
        var result = await handler.Handle(new ExportPatientReportPdfCommand(1, NewTempPdfPath()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Conflict", result.Error!.Code);
        Assert.Equal(0, exporter.CallCount);
        Assert.False(pt.IsExported);
    }

    [Fact]
    public async Task IoFailure_Unexpected_Leaves_ExportFlags_Unchanged()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        var row = VerifiedRow(101, 1, 10);
        db.PatientTests.Add(row);
        var exporter = new FakeReportPdfExporter { ThrowOnExport = true };

        var handler = new ExportPatientReportPdfCommandHandler(db, exporter, new FakeDateTimeProvider());
        var result = await handler.Handle(new ExportPatientReportPdfCommand(1, NewTempPdfPath()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Unexpected", result.Error!.Code);
        Assert.False(row.IsExported);
    }

    [Fact]
    public async Task Export_Has_No_BalanceGate()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddSimpleTest(db, 10);
        var row = VerifiedRow(101, 1, 10);
        db.PatientTests.Add(row);
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(102), PatientId.Create(1), TestId.Create(10), 100m));
        var extra = TopLab.Domain.Billing.PaymentOperation.Create(
            TopLab.Domain.Common.Ids.PaymentOperationId.Create(1), PatientId.Create(1), 500m, 1, DateTime.UtcNow);
        db.PaymentOperations.Add(extra);
        var exporter = new FakeReportPdfExporter();

        var handler = new ExportPatientReportPdfCommandHandler(db, exporter, new FakeDateTimeProvider());

        // Make the second row verified too so the report is eligible despite the balance.
        var second = db.PatientTests.First(p => p.Id.Value == 102);
        second.EnterResult("6", ResultFlag.Normal, 1, DateTime.UtcNow);
        second.MarkReviewed(1, DateTime.UtcNow);

        var result = await handler.Handle(new ExportPatientReportPdfCommand(1, NewTempPdfPath()), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
