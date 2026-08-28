using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Results;

public sealed class CultureAntibioticResult : Entity<CultureAntibioticResultId>
{
    public PatientTestId PatientTestId { get; private set; } = default!;

    public AntibioticId AntibioticId { get; private set; } = default!;

    public SensitivityCategory SensitivityCategory { get; private set; }

    private CultureAntibioticResult()
    {
    }

    private CultureAntibioticResult(CultureAntibioticResultId id, PatientTestId patientTestId, AntibioticId antibioticId, SensitivityCategory sensitivityCategory)
        : base(id)
    {
        PatientTestId = patientTestId;
        AntibioticId = antibioticId;
        SensitivityCategory = sensitivityCategory;
    }

    public static CultureAntibioticResult Create(CultureAntibioticResultId id, PatientTestId patientTestId, AntibioticId antibioticId, SensitivityCategory sensitivityCategory)
    {
        return new CultureAntibioticResult(id, patientTestId, antibioticId, sensitivityCategory);
    }
}
