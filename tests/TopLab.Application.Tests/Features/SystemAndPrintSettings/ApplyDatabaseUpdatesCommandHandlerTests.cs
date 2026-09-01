using TopLab.Application.Features.SystemAndPrintSettings.Commands.ApplyDatabaseUpdates;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public sealed class ApplyDatabaseUpdatesCommandHandlerTests
{
    [Fact]
    public async Task Handle_FullSeedSet_LeavesEverythingUntouched()
    {
        var db = FakeSeedContext();
        var maintenance = new FakeDatabaseMaintenanceService { MigrationsApplied = 2 };
        var handler = new ApplyDatabaseUpdatesCommandHandler(maintenance, db);

        var result = await handler.Handle(new ApplyDatabaseUpdatesCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error?.Message);
        Assert.Equal(2, result.Value!.MigrationsApplied);
        Assert.Equal(0, result.Value.SeedRowsInserted);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_MissingSystemSettingsRow_InsertsExactlyThatRow()
    {
        var db = FakeSeedContext();
        db.SystemSettings.Clear();
        var maintenance = new FakeDatabaseMaintenanceService { MigrationsApplied = 0 };
        var handler = new ApplyDatabaseUpdatesCommandHandler(maintenance, db);

        var result = await handler.Handle(new ApplyDatabaseUpdatesCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error?.Message);
        Assert.Equal(1, result.Value!.SeedRowsInserted);
        Assert.Single(db.SystemSettings);
        Assert.Equal(1, db.SystemSettings[0].Id);
        Assert.Equal(AccountType.Individual, db.SystemSettings[0].DefaultAccountType);
        Assert.Equal(4, db.EnvelopePrintItemPositions.Count);
        Assert.Equal(4, db.PrinterAssignments.Count);
    }

    private static FakeApplicationDbContext FakeSeedContext()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        db.ReceiptSettings.Add(ReceiptSettings.CreateDefault());
        db.EnvelopeSettings.Add(EnvelopeSettings.CreateDefault());
        db.EnvelopePrintItemPositions.Add(new EnvelopePrintItemPosition("Name", true, 1.0m, 1.0m));
        db.EnvelopePrintItemPositions.Add(new EnvelopePrintItemPosition("Code", true, 1.0m, 2.0m));
        db.EnvelopePrintItemPositions.Add(new EnvelopePrintItemPosition("ReferralEntity", true, 1.0m, 3.0m));
        db.EnvelopePrintItemPositions.Add(new EnvelopePrintItemPosition("Date", true, 1.0m, 4.0m));
        db.PrinterAssignments.Add(new PrinterAssignment(PrinterOutputType.Reports, "Reports"));
        db.PrinterAssignments.Add(new PrinterAssignment(PrinterOutputType.Barcode, "Barcode"));
        db.PrinterAssignments.Add(new PrinterAssignment(PrinterOutputType.Envelope, "Envelope"));
        db.PrinterAssignments.Add(new PrinterAssignment(PrinterOutputType.Receipt, "Receipt"));
        return db;
    }
}