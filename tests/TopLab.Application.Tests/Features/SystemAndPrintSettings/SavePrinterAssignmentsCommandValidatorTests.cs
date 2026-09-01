using TopLab.Application.Features.SystemAndPrintSettings.Commands.SavePrinterAssignments;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Common.Enums;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class SavePrinterAssignmentsCommandValidatorTests
{
    private readonly SavePrinterAssignmentsCommandValidator _validator = new();

    private static IReadOnlyList<PrinterAssignmentDto> Assignments() =>
    [
        new(PrinterOutputType.Reports, "Reports"),
        new(PrinterOutputType.Barcode, "Barcode"),
        new(PrinterOutputType.Envelope, "Envelope"),
        new(PrinterOutputType.Receipt, "Receipt")
    ];

    [Fact]
    public void Valid_Passes()
    {
        var result = _validator.Validate(new SavePrinterAssignmentsCommand(Assignments()));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MissingOutputType_Fails()
    {
        var assignments = Assignments().Take(3).ToList();
        var result = _validator.Validate(new SavePrinterAssignmentsCommand(assignments));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void EmptyPrinterName_Fails()
    {
        var assignments = new List<PrinterAssignmentDto>
        {
            new(PrinterOutputType.Reports, ""),
            new(PrinterOutputType.Barcode, "Barcode"),
            new(PrinterOutputType.Envelope, "Envelope"),
            new(PrinterOutputType.Receipt, "Receipt")
        };
        var result = _validator.Validate(new SavePrinterAssignmentsCommand(assignments));
        Assert.False(result.IsValid);
    }
}