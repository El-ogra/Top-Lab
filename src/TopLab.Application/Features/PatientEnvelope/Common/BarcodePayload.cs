using System.Globalization;

namespace TopLab.Application.Features.PatientEnvelope.Common;

/// <summary>
/// Single identifier rule for every Phase 1 scannable surface (Phase 1,
/// REF-066 — shared owner; consumed by REF-066/067/068/127).
/// LabId when the <c>PrintLabIdInsteadOfPatientId</c> flag is on and a
/// non-empty LabId exists, otherwise the invariant-culture PatientId.
/// Precedent: <c>PrintBarcodeCommandHandler</c> lines 40-42.
/// </summary>
public static class BarcodePayload
{
    public static string For(int patientId, string? labId, bool printLabIdInstead)
    {
        if (printLabIdInstead && !string.IsNullOrWhiteSpace(labId))
        {
            return labId;
        }

        return patientId.ToString(CultureInfo.InvariantCulture);
    }
}
