using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.ApplyDatabaseUpdates;

public sealed class ApplyDatabaseUpdatesCommandHandler : IRequestHandler<ApplyDatabaseUpdatesCommand, Result<ApplyDatabaseUpdatesOutcome>>
{
    private readonly IDatabaseMaintenanceService _maintenance;
    private readonly IApplicationDbContext _db;

    public ApplyDatabaseUpdatesCommandHandler(IDatabaseMaintenanceService maintenance, IApplicationDbContext db)
    {
        _maintenance = maintenance;
        _db = db;
    }

    public async Task<Result<ApplyDatabaseUpdatesOutcome>> Handle(ApplyDatabaseUpdatesCommand request, CancellationToken cancellationToken)
    {
        var migrationsResult = await _maintenance.ApplyPendingUpdatesAsync(cancellationToken);
        if (!migrationsResult.IsSuccess)
        {
            return Result<ApplyDatabaseUpdatesOutcome>.Failure(migrationsResult.Errors);
        }

        var inserted = await RunSeedRepairAsync(cancellationToken);

        return Result<ApplyDatabaseUpdatesOutcome>.Success(new ApplyDatabaseUpdatesOutcome(migrationsResult.Value, inserted));
    }

    private async Task<int> RunSeedRepairAsync(CancellationToken cancellationToken)
    {
        var inserted = 0;

        inserted += EnsureSingleRow(() => SystemSettings.CreateDefault(), s => s.Id == 1);
        inserted += EnsureSingleRow(() => ReportSettings.CreateDefault(), r => r.Id == 1);
        inserted += EnsureSingleRow(() => ReceiptSettings.CreateDefault(), r => r.Id == 1);
        inserted += EnsureSingleRow(() => EnvelopeSettings.CreateDefault(), e => e.Id == 1);

        inserted += EnsureMissingPositions();
        inserted += EnsureMissingPrinterAssignments();

        if (inserted > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return inserted;
    }

    private int EnsureSingleRow<T>(Func<T> factory, Func<T, bool> exists) where T : class
    {
        if (_db.Set<T>().Any(exists))
        {
            return 0;
        }

        _db.Add(factory());
        return 1;
    }

    private int EnsureMissingPositions()
    {
        var seeds = new[]
        {
            new EnvelopePrintItemPosition("Name", true, 1.0m, 1.0m),
            new EnvelopePrintItemPosition("Code", true, 1.0m, 2.0m),
            new EnvelopePrintItemPosition("ReferralEntity", true, 1.0m, 3.0m),
            new EnvelopePrintItemPosition("Date", true, 1.0m, 4.0m)
        };

        var existing = _db.Set<EnvelopePrintItemPosition>()
            .Select(p => p.ItemName)
            .ToHashSet();

        var inserted = 0;
        foreach (var seed in seeds.Where(s => !existing.Contains(s.ItemName)))
        {
            _db.Add(seed);
            inserted++;
        }

        return inserted;
    }

    private int EnsureMissingPrinterAssignments()
    {
        var seeds = new[]
        {
            new PrinterAssignment(PrinterOutputType.Reports, "Reports"),
            new PrinterAssignment(PrinterOutputType.Barcode, "Barcode"),
            new PrinterAssignment(PrinterOutputType.Envelope, "Envelope"),
            new PrinterAssignment(PrinterOutputType.Receipt, "Receipt")
        };

        var existing = _db.Set<PrinterAssignment>()
            .Select(p => p.OutputType)
            .ToHashSet();

        var inserted = 0;
        foreach (var seed in seeds.Where(s => !existing.Contains(s.OutputType)))
        {
            _db.Add(seed);
            inserted++;
        }

        return inserted;
    }
}