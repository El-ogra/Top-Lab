using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Domain.PatientStatus;

/// <summary>
/// Stateless domain service implementing the settled seven-state patient aggregate
/// status (PRD §8.2/§8.3, BR-01, ADR-0015). Computed, never stored, never cached.
/// Per-analysis stage from the lifecycle columns (entry → review → print →
/// delivery), account stage from the injected <c>balance</c>, and
/// <c>PatientStatus = state(min over all per-analysis stages AND the account
/// condition)</c> in lifecycle precedence order. S1 applies iff no analysis has
/// <c>EnteredAtUtc</c> set AND the patient's <c>RegistrationDateUtc</c> falls on
/// the current UTC day; otherwise unentered ⇒ S2.
/// </summary>
public sealed class PatientStatusCalculator
{
    public PatientAggregateStatus Calculate(Patient patient, IReadOnlyList<PatientTest> tests, decimal balance)
    {
        ArgumentNullException.ThrowIfNull(patient);
        ArgumentNullException.ThrowIfNull(tests);

        if (tests.Count == 0)
        {
            return IsRegistrationToday(patient.RegistrationDateUtc)
                ? PatientAggregateStatus.S1
                : PatientAggregateStatus.S2;
        }

        var minStage = int.MaxValue;
        foreach (var pt in tests)
        {
            int stage;
            if (pt.EnteredAtUtc is null)
            {
                stage = 1;
            }
            else if (!pt.IsReviewed)
            {
                stage = 2;
            }
            else if (!pt.IsPrinted)
            {
                stage = 3;
            }
            else if (!pt.IsDelivered)
            {
                stage = 4;
            }
            else
            {
                stage = 6;
            }

            if (stage < minStage)
            {
                minStage = stage;
            }
        }

        if (minStage <= 4)
        {
            if (minStage == 1)
            {
                var noneEntered = true;
                foreach (var pt in tests)
                {
                    if (pt.EnteredAtUtc is not null)
                    {
                        noneEntered = false;
                        break;
                    }
                }

                if (noneEntered && IsRegistrationToday(patient.RegistrationDateUtc))
                {
                    return PatientAggregateStatus.S1;
                }

                return PatientAggregateStatus.S2;
            }

            // Stage 2 → S3, stage 3 → S4, stage 4 → S5.
            return (PatientAggregateStatus)(minStage + 1);
        }

        // All results delivered: the S6 account condition applies here only.
        // Delivery precedes settlement: an undelivered analysis with an outstanding
        // balance already returned S5 above.
        return balance > 0 ? PatientAggregateStatus.S6 : PatientAggregateStatus.S7;
    }

    private static bool IsRegistrationToday(DateTime registrationDateUtc)
    {
        return registrationDateUtc.Date == DateTime.UtcNow.Date;
    }
}
