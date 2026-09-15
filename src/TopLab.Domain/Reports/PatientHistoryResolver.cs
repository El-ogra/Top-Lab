using TopLab.Domain.Common.Enums;

namespace TopLab.Domain.Reports;

/// <summary>
/// Resolves the patient-identity key used to gather history lines, per <see cref="HistorySortMode"/>.
/// ByLabCode keys on the LabId; ByPatientName keys on the exact normalized full name (trim + case-fold).
/// </summary>
public static class PatientHistoryResolver
{
    public static string ResolveKey(HistorySortMode mode, string? labId, string fullName)
    {
        return mode switch
        {
            HistorySortMode.ByLabCode => ResolveByLabCode(labId),
            HistorySortMode.ByPatientName => ResolveByPatientName(fullName),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), "Unknown history sort mode.")
        };
    }

    private static string ResolveByLabCode(string? labId)
    {
        if (string.IsNullOrWhiteSpace(labId))
        {
            throw new ArgumentException("A LabId is required to resolve history by lab code.", nameof(labId));
        }

        return labId.Trim();
    }

    private static string ResolveByPatientName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("A full name is required to resolve history by patient name.", nameof(fullName));
        }

        string normalized = string.Join(" ", fullName.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return normalized.ToUpperInvariant();
    }
}