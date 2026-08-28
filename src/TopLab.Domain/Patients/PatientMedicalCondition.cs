using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Patients;

/// <summary>
/// Join between Patient and MedicalConditionType. Composite PK.
/// </summary>
public sealed class PatientMedicalCondition
{
    public PatientId PatientId { get; private set; } = default!;

    public MedicalConditionTypeId MedicalConditionTypeId { get; private set; } = default!;

    private PatientMedicalCondition()
    {
    }

    public PatientMedicalCondition(PatientId patientId, MedicalConditionTypeId medicalConditionTypeId)
    {
        PatientId = patientId;
        MedicalConditionTypeId = medicalConditionTypeId;
    }
}
