using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Domain.Tests.Settings;

public class PrinterAssignmentTests
{
    [Fact]
    public void ChangePrinter_Valid_Updates()
    {
        var p = new PrinterAssignment(PrinterOutputType.Reports, "Reports");
        p.ChangePrinter("HP LaserJet 1018");
        Assert.Equal("HP LaserJet 1018", p.PrinterName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangePrinter_Empty_Throws(string? printerName)
    {
        var p = new PrinterAssignment(PrinterOutputType.Barcode, "Barcode");
        Assert.Throws<ArgumentException>(() => p.ChangePrinter(printerName!));
    }

    [Fact]
    public void ChangePrinter_Overlong_Throws()
    {
        var p = new PrinterAssignment(PrinterOutputType.Envelope, "Envelope");
        var printerName = new string('x', 201);
        Assert.Throws<ArgumentException>(() => p.ChangePrinter(printerName));
    }

    [Fact]
    public void OutputType_IsImmutable()
    {
        var p = new PrinterAssignment(PrinterOutputType.Receipt, "Receipt");
        Assert.Equal(PrinterOutputType.Receipt, p.OutputType);
    }
}