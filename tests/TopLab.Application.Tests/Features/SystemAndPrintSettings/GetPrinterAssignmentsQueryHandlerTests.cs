using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetPrinterAssignments;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class GetPrinterAssignmentsQueryHandlerTests
{
    [Fact]
    public async Task GetPrinterAssignments_ReturnsFourSeededRows()
    {
        var db = new FakeApplicationDbContext();
        db.PrinterAssignments.Add(new PrinterAssignment(PrinterOutputType.Reports, "Reports"));
        db.PrinterAssignments.Add(new PrinterAssignment(PrinterOutputType.Barcode, "Barcode"));
        db.PrinterAssignments.Add(new PrinterAssignment(PrinterOutputType.Envelope, "Envelope"));
        db.PrinterAssignments.Add(new PrinterAssignment(PrinterOutputType.Receipt, "Receipt"));

        var handler = new GetPrinterAssignmentsQueryHandler(db);
        var result = await handler.Handle(new GetPrinterAssignmentsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value!.Count);
        Assert.Contains(result.Value, p => p.OutputType == PrinterOutputType.Reports && p.PrinterName == "Reports");
        Assert.Contains(result.Value, p => p.OutputType == PrinterOutputType.Barcode && p.PrinterName == "Barcode");
        Assert.Contains(result.Value, p => p.OutputType == PrinterOutputType.Envelope && p.PrinterName == "Envelope");
        Assert.Contains(result.Value, p => p.OutputType == PrinterOutputType.Receipt && p.PrinterName == "Receipt");
    }
}