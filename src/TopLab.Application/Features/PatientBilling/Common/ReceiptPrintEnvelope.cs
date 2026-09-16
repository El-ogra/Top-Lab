using System.Text.Json;

namespace TopLab.Application.Features.PatientBilling.Common;

/// <summary>
/// Self-contained receipt print payload handed to
/// <c>IReceiptPrintingService</c> through its single string token: the
/// JSON-serialized <c>ReceiptDto</c> produced by <c>GetPatientReceiptQuery</c>.
/// Mirrors <c>ReportPrintEnvelope</c> — Application stays dependency-free and
/// the port surface stays thin.
/// </summary>
public sealed record ReceiptPrintEnvelope(string ReceiptJson)
{
    public static string CreateToken(ReceiptDto receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        return JsonSerializer.Serialize(new ReceiptPrintEnvelope(JsonSerializer.Serialize(receipt)));
    }
}
