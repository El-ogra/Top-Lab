using System.Text.Json;

namespace TopLab.Application.Features.ResultDelivery.Common;

/// <summary>
/// Self-contained group-slip print payload handed to
/// <c>IGroupPatientSlipPrintingService</c> through its single string token: the
/// JSON-serialized <c>GroupPatientSlipBatchDto</c> produced by
/// <c>PrintGroupPatientSlipsCommandHandler</c>.
/// Mirrors <c>LabOrderPrintEnvelope</c> / <c>ReceiptPrintEnvelope</c> —
/// Application stays dependency-free and the port surface stays thin.
/// </summary>
public sealed record GroupPatientSlipPrintEnvelope(string BatchJson)
{
    public static string CreateToken(GroupPatientSlipBatchDto batch)
    {
        ArgumentNullException.ThrowIfNull(batch);
        return JsonSerializer.Serialize(new GroupPatientSlipPrintEnvelope(JsonSerializer.Serialize(batch)));
    }
}
