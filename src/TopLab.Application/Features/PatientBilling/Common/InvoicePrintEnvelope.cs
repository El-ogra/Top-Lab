using System.Text.Json;

namespace TopLab.Application.Features.PatientBilling.Common;

/// <summary>
/// Self-contained invoice print payload handed to
/// <c>IInvoicePrintingService</c> through its single string token: the
/// JSON-serialized <c>InvoiceDto</c> produced by <c>GetPatientInvoiceQuery</c>
/// (preview) or <c>PrintInvoiceCommand</c> (numbered issue).
/// Mirrors <c>ReceiptPrintEnvelope</c> — Application stays dependency-free and
/// the port surface stays thin.
/// </summary>
public sealed record InvoicePrintEnvelope(string InvoiceJson)
{
    public static string CreateToken(InvoiceDto invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        return JsonSerializer.Serialize(new InvoicePrintEnvelope(JsonSerializer.Serialize(invoice)));
    }
}
