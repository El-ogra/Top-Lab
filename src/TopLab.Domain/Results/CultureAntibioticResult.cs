using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Results;

public sealed class CultureAntibioticResult : Entity<CultureAntibioticResultId>
{
    public PatientTestId PatientTestId { get; private set; } = default!;

    public AntibioticId AntibioticId { get; private set; } = default!;

    /// <summary>Nullable after WP-03 repair (0→NULL / Unspecified); values 0–3 unchanged (SD-9).</summary>
    public SensitivityCategory? SensitivityCategory { get; private set; }

    private CultureAntibioticResult()
    {
    }

    private CultureAntibioticResult(CultureAntibioticResultId id, PatientTestId patientTestId, AntibioticId antibioticId, SensitivityCategory? sensitivityCategory)
        : base(id)
    {
        PatientTestId = patientTestId;
        AntibioticId = antibioticId;
        SensitivityCategory = sensitivityCategory;
    }

    public static CultureAntibioticResult Create(CultureAntibioticResultId id, PatientTestId patientTestId, AntibioticId antibioticId, SensitivityCategory? sensitivityCategory)
        {
            return new CultureAntibioticResult(id, patientTestId, antibioticId, sensitivityCategory);
        }

        /// <summary>W-02 C-19/SD-13: changes only the category, so a re-save keeps the row's identity
        /// (and, from S9, any column already stored on it such as the inhibition zone).</summary>
        public void UpdateSensitivity(SensitivityCategory? value)
        {
            SensitivityCategory = value;
        }
    }
