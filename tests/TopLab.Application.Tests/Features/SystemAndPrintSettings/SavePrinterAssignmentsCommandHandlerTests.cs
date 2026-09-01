using TopLab.Application.Features.SystemAndPrintSettings.Commands.SavePrinterAssignments;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class SavePrinterAssignmentsCommandHandlerTests
{
    private static void Seed(FakeApplicationDbContext db)
    {
        db.PrinterAssignments.Add(new PrinterAssignment(PrinterOutputType.Reports, "Reports"));
        db.PrinterAssignments.Add(new PrinterAssignment(PrinterOutputType.Barcode, "Barcode"));
        db.PrinterAssignments.Add(new PrinterAssignment(PrinterOutputType.Envelope, "Envelope"));
        db.PrinterAssignments.Add(new PrinterAssignment(PrinterOutputType.Receipt, "Receipt"));
    }

    private static IReadOnlyList<PrinterAssignmentDto> Assignments() =>
    [
        new(PrinterOutputType.Reports, "HP LaserJet"),
        new(PrinterOutputType.Barcode, "TSC TTP-247"),
        new(PrinterOutputType.Envelope, "HP LaserJet"),
        new(PrinterOutputType.Receipt, "Epson TM-U220")
    ];

    [Fact]
    public async Task SavePrinterAssignments_ReplacesFourNames()
    {
        var db = new FakeApplicationDbContext();
        Seed(db);

        var handler = new SavePrinterAssignmentsCommandHandler(db);
        var result = await handler.Handle(new SavePrinterAssignmentsCommand(Assignments()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("HP LaserJet", db.PrinterAssignments.Single(p => p.OutputType == PrinterOutputType.Reports).PrinterName);
        Assert.Equal("TSC TTP-247", db.PrinterAssignments.Single(p => p.OutputType == PrinterOutputType.Barcode).PrinterName);
        Assert.Equal("HP LaserJet", db.PrinterAssignments.Single(p => p.OutputType == PrinterOutputType.Envelope).PrinterName);
        Assert.Equal("Epson TM-U220", db.PrinterAssignments.Single(p => p.OutputType == PrinterOutputType.Receipt).PrinterName);
    }

    [Fact]
    public async Task SavePrinterAssignments_DoesNotInsertOrDeleteRows()
    {
        var db = new FakeApplicationDbContext();
        Seed(db);

        var handler = new SavePrinterAssignmentsCommandHandler(db);
        var result = await handler.Handle(new SavePrinterAssignmentsCommand(Assignments()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, db.PrinterAssignments.Count);
    }
}