using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Domain.PatientStatus;

/// <summary>
/// Stateless domain service. Full precedence delivered in M04 per Master_Tracking_Sheet.
/// TODO: implement seven-state min-over-stages + account check when M04 ships.
/// </summary>
public sealed class PatientStatusCalculator
{
    public PatientAggregateStatus Calculate(Patient patient, IReadOnlyList<PatientTest> tests, decimal balance)
    {
        // Stub — real logic in M04. Keeps type available for F5 schema baseline.
        throw new NotImplementedException("Full precedence delivered in M04 per Master Tracking Sheet Cross-Cutting Concern (BR-01, ADR-0015).");
    }
}
