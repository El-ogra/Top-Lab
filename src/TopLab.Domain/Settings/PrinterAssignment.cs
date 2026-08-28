using TopLab.Domain.Common.Enums;

namespace TopLab.Domain.Settings;

/// <summary>PK is OutputType (byte).</summary>
public sealed class PrinterAssignment
{
    public PrinterOutputType OutputType { get; private set; }

    public string PrinterName { get; private set; } = default!;

    private PrinterAssignment()
    {
    }

    public PrinterAssignment(PrinterOutputType outputType, string printerName)
    {
        OutputType = outputType;
        PrinterName = printerName;
    }
}
