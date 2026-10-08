using System.Text.Json;

namespace TopLab.Application.Features.PatientEnvelope.Common;

/// <summary>
/// Self-contained lab-order print payload handed to
/// <c>ILabOrderPrintingService</c> through its single string token: the
/// JSON-serialized <c>LabOrderDto</c> produced by
/// <c>PrintLabOrderCommandHandler</c>.
/// Mirrors <c>ReceiptPrintEnvelope</c> — Application stays dependency-free and
/// the port surface stays thin.
/// </summary>
public sealed record LabOrderPrintEnvelope(string LabOrderJson)
{
    public static string CreateToken(LabOrderDto order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return JsonSerializer.Serialize(new LabOrderPrintEnvelope(JsonSerializer.Serialize(order)));
    }
}
