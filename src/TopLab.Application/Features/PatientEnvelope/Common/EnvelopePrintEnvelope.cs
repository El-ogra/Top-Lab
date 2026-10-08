using System.Text.Json;

namespace TopLab.Application.Features.PatientEnvelope.Common;

/// <summary>
/// Self-contained envelope print payload handed to
/// <c>IEnvelopePrintingService</c> through its single string token: the
/// JSON-serialized <c>EnvelopeDto</c> produced by
/// <c>PrintEnvelopeCommandHandler</c>.
/// Mirrors <c>ReceiptPrintEnvelope</c> — Application stays dependency-free and
/// the port surface stays thin.
/// </summary>
public sealed record EnvelopePrintEnvelope(string EnvelopeJson)
{
    public static string CreateToken(EnvelopeDto envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        return JsonSerializer.Serialize(new EnvelopePrintEnvelope(JsonSerializer.Serialize(envelope)));
    }
}
